using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스캔된 블루투스 기기의 정보를 화면에 표시하기 위한 클래스
public class Ble_ScannedItemScript : MonoBehaviour
{
	// 블루투스 기기의 이름, 주소, RSSI 값을 표시하기 위한 UI Text 요소
	public TextMeshProUGUI TextNameValue; // 기기 이름을 위한 UI Text 요소의 참조
	public TextMeshProUGUI TextAddressValue; // 기기 주소를 위한 UI Text 요소의 참조
	public TextMeshProUGUI TextRSSIValue; // RSSI 값을 위한 UI Text 요소의 참조

	// 블루투스 기기의 이름을 설정하는 메서드
	public void setNameValue(string value)
	{
		TextNameValue.text = value; // 입력된 값을 TextNameValue UI 요소에 할당합니다.
	}

	// 블루투스 기기의 주소를 설정하는 메서드
	public void setAddressValue(string value)
	{
		TextAddressValue.text = value; // 입력된 값을 TextAddressValue UI 요소에 할당합니다.
	}

	// 블루투스 기기의 RSSI(수신 신호 세기 지표) 값을 설정하는 메서드
	public void setRssiValue(string value)
	{
		TextRSSIValue.text = value; // 입력된 값을 TextRSSIValue UI 요소에 할당합니다.
	}

}