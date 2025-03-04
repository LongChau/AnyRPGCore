using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CombatTextController : MonoBehaviour {
        //[SerializeField]
        //private TextMeshProUGUI tmpProtext = null;

        [SerializeField]
        private TextMeshProUGUI tmpProtext = null;

        [SerializeField]
        private Image image = null;

        [SerializeField]
        private float xUIOffset = 50f;

        [SerializeField]
        private float yUnitOffset = 2.2f;

        // pixels to move over the total fade time
        //[SerializeField]
        private float distanceToMove = 200f;

        [SerializeField]
        private float fadeTime = 3f;

        [SerializeField]
        private int defaultFontSize = 30;

        private string displayText = string.Empty;
        private Interactable mainTarget = null;
        private float alpha;
        private Vector2 targetPos;
        private float fadeOutTimer;
        private float fadeRate;
        private Color textColor;
        private CombatMagnitude combatMagnitude;
        private CombatTextType textType;
        private AbilityEffectContext abilityEffectContext = null;

        private float randomXLimit = 100f;
        private float randomYLimit = 100f;
        private float randomX;
        private float randomY;

        // change direction to downward text for hits against player
        private int directionMultiplier = 1;

        public void InitializeCombatTextController(Interactable mainTarget, Sprite sprite, string displayText, CombatTextType combatTextType, CombatMagnitude combatMagnitude = CombatMagnitude.normal, AbilityEffectContext abilityEffectContext = null) {
            this.mainTarget = mainTarget;
            image.sprite = sprite;
            this.displayText = displayText;
            this.textType = combatTextType;
            this.combatMagnitude = combatMagnitude;
            this.abilityEffectContext = abilityEffectContext;

            gameObject.SetActive(true);

            // if the combat text ui is not active, then we should just immediately disable this
            if (gameObject.activeInHierarchy == false) {
                CombatTextManager.MyInstance.returnControllerToPool(this);
                return;
            }

            //Debug.Log("Combat Text spawning: " + textType);
            randomX = Random.Range(0, randomXLimit);
            randomY = Random.Range(0, randomYLimit);
            //Debug.Log("Combat Text spawning: " + textType + "; randomX: " + randomX + "; randomY: " + randomY);
            targetPos = CameraManager.MyInstance.MyActiveMainCamera.WorldToScreenPoint(mainTarget.InteractableGameObject.transform.position);
            //alpha = text.color.a;
            alpha = 1f;
            tmpProtext.fontSize = defaultFontSize;
            fadeOutTimer = fadeTime;
            fadeRate = 1.0f / fadeTime;
            directionMultiplier = 1;

            string preText = string.Empty;
            string postText = string.Empty;

            if (image.sprite == null) {
                image.color = new Color32(0, 0, 0, 0);
            } else {
                image.color = Color.white;
            }
            if (mainTarget.InteractableGameObject == PlayerManager.MyInstance.ActiveUnitController.gameObject) {
                directionMultiplier = -1;
                switch (textType) {
                    case CombatTextType.normal:
                        textColor = Color.red;
                        int parseResult;
                        if (int.TryParse(displayText, out parseResult)) {
                            preText += parseResult > 0 ? "-" : "";
                        }
                        break;
                    case CombatTextType.gainXP:
                        textColor = Color.yellow;
                        preText += "+";
                        postText += " XP";
                        tmpProtext.fontSize = tmpProtext.fontSize * 2;
                        break;
                    case CombatTextType.gainBuff:
                        textColor = Color.cyan;
                        preText += "+";
                        //text.fontSize = text.fontSize * 2;
                        break;
                    case CombatTextType.loseBuff:
                        textColor = Color.cyan;
                        preText += "+";
                        //text.fontSize = text.fontSize * 2;
                        break;
                    case CombatTextType.ability:
                        textColor = Color.magenta;
                        preText += "-";
                        tmpProtext.fontSize = tmpProtext.fontSize * 2;
                        break;
                    default:
                        break;
                }
            } else {
                switch (textType) {
                    case CombatTextType.normal:
                        textColor = Color.white;
                        break;
                    case CombatTextType.ability:
                        textColor = Color.yellow;
                        tmpProtext.fontSize = tmpProtext.fontSize * 2;
                        break;
                    default:
                        break;
                }
            }
            // defaults
            switch (textType) {
                case CombatTextType.gainHealth:
                    textColor = Color.green;
                    preText += "+";
                    tmpProtext.fontSize = tmpProtext.fontSize * 2;
                    break;
                case CombatTextType.miss:
                    textColor = Color.white;
                    //preText += "";
                    //text.fontSize = text.fontSize * 2;
                    break;
                case CombatTextType.immune:
                    textColor = Color.white;
                    //preText += "";
                    //text.fontSize = text.fontSize * 2;
                    break;
                case CombatTextType.gainResource:
                    if (abilityEffectContext?.powerResource != null) {
                        textColor = abilityEffectContext.powerResource.DisplayColor;
                        postText += " " + abilityEffectContext.powerResource.DisplayName;
                    } else {
                        textColor = Color.blue;
                    }
                    preText += "+";
                    // rage looks funny bigger
                    //tmpProtext.fontSize = tmpProtext.fontSize * 2;
                    break;
                default:
                    break;
            }

            tmpProtext.color = textColor;
            string finalString = preText + displayText + postText;
            tmpProtext.text = finalString;
            if (combatMagnitude == CombatMagnitude.critical) {
                tmpProtext.fontSize = tmpProtext.fontSize * 2;
            }
            RunCombatTextUpdate();
        }

        public void RunCombatTextUpdate() {
            //Debug.Log("CombatTextController.FixedUpdate()");
            if (mainTarget != null) {
                //Debug.Log("CombatTextController.FixedUpdate(): maintarget is not null");
                targetPos = CameraManager.MyInstance.MyActiveMainCamera.WorldToScreenPoint(mainTarget.InteractableGameObject.transform.position + new Vector3(0, yUnitOffset, 0));
                //Debug.Log("CombatTextController.FixedUpdate(): targetpos:" + targetPos);
                transform.position = targetPos + new Vector2(randomX + xUIOffset, randomY);
            } else {
                //Debug.Log("CombatTextController.FixedUpdate(): maintarget is null");
            }
            if (fadeOutTimer > 0) {
                fadeOutTimer -= Time.deltaTime;

                alpha -= fadeRate * Time.deltaTime;

                // fade text
                Color tmp = tmpProtext.color;
                tmp.a = alpha;
                tmpProtext.color = tmp;

                // fade image
                if (image.sprite != null) {
                    Color imageColor = image.color;
                    imageColor.a = alpha;
                    image.color = imageColor;
                }

                //randomY += (movementSpeed * directionMultiplier);
                randomY = distanceToMove * (((fadeOutTimer - fadeTime) * -1) / fadeTime) * directionMultiplier;
            } else {
                CombatTextManager.MyInstance.returnControllerToPool(this);
                //Destroy(this.gameObject);
            }

        }

        /*
        void FixedUpdate() {
            RunCombatTextUpdate();
        }
        */
    }

}