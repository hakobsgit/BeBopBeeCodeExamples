using System;
using System.Collections;
using LessonV2.Data.Enums;
using LessonV2.Data.Facades;
using LessonV2.Data.ScriptableObjects;
using LessonV2.Data.Stores;
using LessonV2.Interfaces;
using LessonV2.Steps;
using LessonV2.Views;
using UniRx;
using UnityEngine;

namespace LessonV2.Controllers {
    public class LessonStepController {
        private readonly GameFacade _gameFacade;
        private readonly LessonFacade _facade;
        private readonly LessonObjectsView _lessonObjects;
        private readonly LessonDataStore _dataStore;
        private LessonEntity _entity;
        private LessonStepEntity _currentStepEntity;
        private IDisposable _currentStepDisposable;
        private ILessonStep _currentStep;

        public LessonStepController(GameFacade gameFacade, LessonFacade lessonFacade, LessonObjectsView lessonObjects, LessonEntity entity, LessonDataStore dataStore) {
            _gameFacade = gameFacade;
            _facade = lessonFacade;
            _lessonObjects = lessonObjects;
            _entity = entity;
            _dataStore = dataStore;
        }

        public void DisposeCurrentStep() {
            _currentStepDisposable?.Dispose();
        }

        public void TerminateCurrentStep() {
            _currentStep?.Terminate();
        }

        public IEnumerator ExecuteStep(LessonStepEntity stepEntity) {
            _currentStepEntity = stepEntity;
            SetupByCurrentStepData();
            _currentStep = null;
            switch (stepEntity.Type) {
                case LessonStepEntityType.Default:
                    _currentStep = new LessonDefaultStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity, null, _dataStore);
                    break;
                case LessonStepEntityType.Action:
                    _currentStep = new LessonActionStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity, stepEntity.ActionEntity, _dataStore);
                    break;
                case LessonStepEntityType.Speech:
                    _currentStep = new LessonSpeechStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.SpeechEntity, _dataStore);
                    break;
                case LessonStepEntityType.Play:
                    _currentStep = new LessonPlayStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.PlayEntity, _dataStore);
                    break;
                case LessonStepEntityType.Part:
                    _currentStep = new LessonPartStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.PartEntity, _dataStore);
                    break;
                case LessonStepEntityType.Rule:
                    _currentStep = new LessonRuleStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.RuleEntity, _dataStore);
                    break;
                case LessonStepEntityType.Bid:
                    _currentStep = new LessonBidStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.BidEntity, _dataStore);
                    break;
                case LessonStepEntityType.BidSelection:
                    _currentStep = new LessonBidSelectionStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.BidSelectionEntity, _dataStore);
                    break;
                case LessonStepEntityType.ContractFly:
                    _currentStep = new LessonContractFlyStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.ContractFlyEntity, _dataStore);
                    break;
                case LessonStepEntityType.Popup:
                    _currentStep = new LessonPopupStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.PopupEntity, _dataStore);
                    break;
                case LessonStepEntityType.Finish:
                    _currentStep = new LessonFinishStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.FinishEntity, _dataStore);
                    break;
                case LessonStepEntityType.BidsPanel:
                    _currentStep = new LessonBidsPanelStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.BidsPanelEntity, _dataStore);
                    break;
                case LessonStepEntityType.NumberSelection:
                    _currentStep = new LessonNumberSelectionStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.NumberSelectionEntity, _dataStore);
                    break;
                case LessonStepEntityType.BidExplanation:
                    _currentStep = new LessonBidExplanationStep().Build(_gameFacade, _facade, _lessonObjects, _entity, stepEntity,  stepEntity.BidExplanationEntity, _dataStore);
                    break;
            }

            if (_currentStep != null) {
                _currentStepDisposable = _currentStep.Execute();
                yield return _currentStepDisposable;
                _currentStep.Disposable.Dispose();
            }
        }

        private void SetupByCurrentStepData() {
            _lessonObjects.Dimmer.enabled = _currentStepEntity.HasDimmer;
            _lessonObjects.BottomInfoText.text = _currentStepEntity.BottomInfoText;
            _lessonObjects.BottomInfoText.gameObject.SetActive(true);
            _lessonObjects.BottomButton.gameObject.SetActive(_currentStepEntity.HasBottomButton);
            _lessonObjects.FullScreenButton.gameObject.SetActive(_currentStepEntity.HasFullScreenButton);
            _lessonObjects.FullScreenButtonText.enabled = !_currentStepEntity.MoveFullScreenButtonTextToAboveCards;
            _lessonObjects.FullScreenButtonTextAboveCards.enabled = _currentStepEntity.MoveFullScreenButtonTextToAboveCards &&
                                                                    !_currentStepEntity.MoveFullScreenButtonTextUnderBiddingBox;

            string fullScreenButtonText = string.IsNullOrEmpty(_currentStepEntity.FullScreenButtonText)
                ? "Tap to continue"
                : _currentStepEntity.FullScreenButtonText;

            _lessonObjects.FullScreenButtonText.text = fullScreenButtonText;
            _lessonObjects.FullScreenButtonTextAboveCards.text = fullScreenButtonText;
            _lessonObjects.FullScreenButtonTextUnderBiddingBox.text = fullScreenButtonText;

            if (_currentStepEntity.HideFullScreenButtonText) {
                _lessonObjects.FullScreenButtonText.enabled = false;
                _lessonObjects.FullScreenButtonTextAboveCards.enabled = false;
                _lessonObjects.FullScreenButtonTextUnderBiddingBox.enabled = false;
            }

            if (_currentStepEntity.HasBottomButton) {
                _lessonObjects.BottomButtonText.text = _currentStepEntity.BottomButtonText;
            }

            if (_currentStepEntity.ShowRuleCustomButton) {
                _lessonObjects.RuleCustomButton.gameObject.SetActive(true);
                _lessonObjects.RuleCustomButtonText.text = _currentStepEntity.RuleCustomButtonText;
                _lessonObjects.RuleCustomButton.onClick.RemoveAllListeners();
                int index = _currentStepEntity.RuleIndexForCustomButton;
                string text = _currentStepEntity.RuleTextForCustomButton;
                _lessonObjects.RuleCustomButton.onClick.AddListener(() =>
                    ShowCustomRule(index, text));
            }

            if (_currentStepEntity.HideRuleCustomButton) {
                _lessonObjects.RuleCustomButton.gameObject.SetActive(false);
            }

            _lessonObjects.InfoBar.SetActive(_currentStepEntity.ShowTricksInfoBar);
            if (_currentStepEntity.ShowUnderRulesText) {
                _lessonObjects.UnderRulesText.text = _currentStepEntity.UnderRulesText;
                _lessonObjects.UnderRulesText.gameObject.SetActive(true);
            }
            else if (_currentStepEntity.HideUnderRulesText) {
                _lessonObjects.UnderRulesText.gameObject.SetActive(false);
            }
        }
        public void ShowCustomRule(int index, string text) {
            Transform posTr = _gameFacade.LessonLessonRules[3].transform;

            PopupManager.Instance.ShowLessonRulePopup(index, text,
                Camera.main.WorldToScreenPoint(posTr.position),
                () => {
                    PopupManager.Instance.HidePopup();
                });
        }
    }
}