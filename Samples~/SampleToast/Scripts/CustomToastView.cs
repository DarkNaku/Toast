using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkNaku.Toast;

public class CustomToastView : ToastView
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Text _textTest;
    
    public void SetMessage(string message)
    {
        _textTest.text = message;
    }

    protected override IEnumerator CoShow()
    {
        yield return CoChangeAlpha(0f, 1f, 0.5f);

        yield return new WaitForSeconds(2.5f);
        
        yield return CoChangeAlpha(1f, 0f, 0.5f);
    }

    private IEnumerator CoChangeAlpha(float start, float end, float duration)
    {
        _canvasGroup.alpha = start;
        
        var elapsedTime = 0f;
        
        while (elapsedTime <= duration)
        {
            _canvasGroup.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}