using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Monopoly.Client.Runtime.UI.Panels.Common;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Global
{
    internal sealed class MessageBoxPanel : MonoBehaviour, IPanel, ICallableAction, ICallableState
    {
        [SerializeField, Header("Canvas"), Space]
        private Canvas canvas;

        [SerializeField]
        private Image imageIcon;

        [SerializeField]
        private TMP_Text textMessage;

        [SerializeField, Header("Canvas Ok"), Space]
        private RectTransform canvasOk;

        [SerializeField]
        private Button buttonOkCanvasOk;

        [SerializeField, Header("Canvas Ok/Cancel"), Space]
        private RectTransform canvasOkCancel;

        [SerializeField]
        private Button buttonOkCanvasOkCancel;

        [SerializeField]
        private Button buttonCancelCanvasOkCancel;

        [SerializeField, Header("Assets"), Space]
        private Sprite spriteError;

        [SerializeField]
        private Sprite spriteTrophy;

        [SerializeField]
        private Sprite spriteWarning;

        [SerializeField]
        private Sprite spriteLoading;

        [SerializeField]
        private Sprite spriteSuccess;

        [SerializeField]
        private Sprite spriteFailure;

        [SerializeField]
        private Sprite spriteQuestion;

        internal enum Type : byte
        {
            OK,
            None,
            OKCancel,
        }

        internal enum Icon : byte
        {
            None,
            Error,
            Trophy,
            Warning,
            Loading,
            Success,
            Failure,
            Question,
        }

        internal enum DialogResult : byte
        {
            OK,
            None,
            Cancel
        }

        private Action actionsHandler;
        private Func<bool> stateHandler;

        internal string MessageBoxText
        {
            set => this.textMessage.text = value;
        }

        internal Type MessageBoxType { get; set; }
        internal DialogResult PanelDialogResult { get; private set; }

        internal Icon MessageBoxIcon
        {
            set
            {
                switch (value)
                {
                    case Icon.None:
                        this.imageIcon.sprite = null;
                        break;
                    case Icon.Error:
                        this.imageIcon.sprite = this.spriteError;
                        break;
                    case Icon.Trophy:
                        this.imageIcon.sprite = this.spriteTrophy;
                        break;
                    case Icon.Warning:
                        this.imageIcon.sprite = this.spriteWarning;
                        break;
                    case Icon.Loading:
                        this.imageIcon.sprite = this.spriteLoading;
                        break;
                    case Icon.Success:
                        this.imageIcon.sprite = this.spriteSuccess;
                        break;
                    case Icon.Failure:
                        this.imageIcon.sprite = this.spriteFailure;
                        break;
                    case Icon.Question:
                        this.imageIcon.sprite = this.spriteQuestion;
                        break;
                }
            }
        }

        private void Start()
        {
            SceneManager.activeSceneChanged += this.OnActiveSceneChanged;

            this.buttonOkCanvasOk.onClick.AddListener(this.OnButtonOkClicked);
            this.buttonOkCanvasOkCancel.onClick.AddListener(this.OnButtonOkClicked);
            this.buttonCancelCanvasOkCancel.onClick.AddListener(this.OnButtonCancelClicked);
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;

            this.buttonOkCanvasOk.onClick.RemoveListener(this.OnButtonOkClicked);
            this.buttonOkCanvasOkCancel.onClick.RemoveListener(this.OnButtonOkClicked);
            this.buttonCancelCanvasOkCancel.onClick.RemoveListener(this.OnButtonCancelClicked);
        }

        private void OnButtonOkClicked()
        {
            this.PanelDialogResult = MessageBoxPanel.DialogResult.OK;
            this.actionsHandler?.Invoke();
            this.Hide();
        }

        private void OnButtonCancelClicked()
        {
            this.PanelDialogResult = MessageBoxPanel.DialogResult.Cancel;
            this.actionsHandler?.Invoke();
            this.Hide();
        }

        private void OnActiveSceneChanged(Scene previousActiveScene, Scene newActiveScene)
        {
            if (this.MessageBoxType == MessageBoxPanel.Type.None)
                this.Hide();
        }

        public void Show(Func<bool> stateCallback = null)
        {
            this.stateHandler = stateCallback;
            this.Show();
        }

        public void Show(Action actionCallback = null)
        {
            this.actionsHandler = actionCallback;
            this.Show();
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
            this.canvas.gameObject.SetActive(true);

            switch (this.MessageBoxType)
            {
                case Type.OK:
                    this.canvasOk.gameObject.SetActive(true);
                    break;
                case Type.None:
                    if (this.stateHandler != null)
                        this.StartCoroutine(this.WaitStateCoroutine());
                    break;
                case Type.OKCancel:
                    this.canvasOkCancel.gameObject.SetActive(true);
                    break;
            }
        }

        private IEnumerator WaitStateCoroutine()
        {
            yield return new WaitUntil(this.stateHandler);
            this.Hide();
        }

        public void Hide()
        {
            this.stateHandler = null;
            this.actionsHandler = null;

            this.gameObject.SetActive(false);
            this.canvas.gameObject.SetActive(false);

            switch (this.MessageBoxType)
            {
                case Type.OK:
                    this.canvasOk.gameObject.SetActive(false);
                    break;
                case Type.OKCancel:
                    this.canvasOkCancel.gameObject.SetActive(false);
                    break;
            }
        }
    }
}
