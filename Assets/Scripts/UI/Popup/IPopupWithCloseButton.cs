using UnityEngine;

namespace UI.Popup
{
    public interface IPopupWithCloseButton
    {
        CustomButtonController CloseButton { get; }
        void OnClickClose();
        void InitializeCloseButton();
    }
}
