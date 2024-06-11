using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkNaku.Toast;

public class ToastTest : MonoBehaviour
{
    private int _count = 0;
    
    public void OnClickToast()
    {
        Toast.Show<CustomToastView>().SetMessage($"[{_count}] 테스트 메세지 입니다.");
        
        _count++;
    }
}