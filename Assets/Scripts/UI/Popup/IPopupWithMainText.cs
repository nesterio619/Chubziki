using TMPro;

namespace UI.Popup
{
    public interface IPopupWithMainText
    {
        TextMeshProUGUI MainText { get; }
        void InitializeMainText(string text);
    }
}
