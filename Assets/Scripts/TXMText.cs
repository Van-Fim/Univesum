using TMPro;
using UnityEngine;
using Zenject;
public class TXMText : TextMeshProUGUI
{
    [SerializeField] private string textCode;

    public string TextCode
    {
        get
        {
            return textCode;
        }
        set
        {
            textCode = value;
            text = LangManager.singleton.ProcessAndReplace(value, code =>
            {
                return $"{code}";
            });
        }
    }

    protected override void Start()
    {
        base.Start();
        textCode = this.text;
        string t = LangManager.singleton.ProcessAndReplace(this.text, code =>
        {
            return $"{code}";
        });
        this.text = t;
    }
}
