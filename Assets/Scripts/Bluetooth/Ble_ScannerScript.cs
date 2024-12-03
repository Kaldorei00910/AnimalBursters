using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine.SceneManagement;
using System.ComponentModel.Design;

// 블루투스 스캐너 기능을 구현하는 클래스
public class Ble_ScannerScript : MonoBehaviour
{
	public GameObject Content; // 스크롤 뷰의 Content 오브젝트
	public GameObject ButtonPrefab2; // 생성할 버튼의 프리팹
	public Text StatusText; // 상태 메시지를 표시할 텍스트
	public Text LeftWheelSpeedDataText; // 왼쪽 바퀴 속도를 표시할 텍스트
	public Text RightWheelSpeedDataText; // 오른쪽 바퀴 속도를 표시할 텍스트


	private float _startScanTimeout = 3f; // 스캔 시작 시 타임아웃 시간
	private float _startScanDelay = 0.1f; // 스캔 시작 시 지연 시간
	private Dictionary<string, Ble_ScannedItemScript> _scannedItems; // 스캔된 아이템을 저장할 딕셔너리
#pragma warning disable IDE0052, CS0414
    private bool _startScan = true; // 스캔 시작 여부
    private bool _foundButtonUUID = false; // 버튼 UUID 발견 여부
	private bool _foundLedUUID = false; // LED UUID 발견 여부
    private float _timeout; // 스캔 타임아웃 변수
#pragma warning restore IDE0052, CS0414
    public double leftCumulativeDistance = 0; // 왼쪽 누적 거리
	public double rightCumulativeDistance = 0; // 오른쪽 누적 거리

	public Toggle WheelyXToggle;
	public BluetoothState State { get; private set; } // 블루투스 상태

    private BluetoothService _service; // 블루투스 서비스 인스턴스

    // 상태 메시지 설정하는 속성
    private string StatusMessage
	{
		set
		{
			// BluetoothLEHardwareInterface.Log(value);

			// 블루투스가 꺼져 있는지 확인
			if (value.ToString() == "BleOff")
			{
                StatusText.text = value.ToString(); // 상태 메시지를 텍스트에 설정
				StatusText.color = Color.red; // 빨간색으로 표시
			}
			else
			{
				Debug.Log(value.ToString());
				StatusText.text = value.ToString(); // 상태 메시지를 텍스트에 설정
				StatusText.color = Color.green; // 초록색으로 표시
			}
		}
	}


	// Awake 메서드 - 인스턴스화될 때 초기화
	void Awake()
	{
		// 블루투스 서비스 초기화
		_service = BluetoothService.Instance;
		_scannedItems = new Dictionary<string, Ble_ScannedItemScript>(); // 딕셔너리 초기화

		StatusMessage = "Init"; // 초기 상태 메시지 설정
	}

	// 활성화될 때 호출되는 메서드
	void OnEnable()
	{
		// 이벤트 핸들러 등록
		_service.OnScanResultsChanged += HandleScanResultsChanged;
		_service.OnSensorAddressChanged += HandleSensorAddressChanged;
		_service.OnIsScanningChanged += HandleIsScanningChanged;
		_service.OnLeftWheelyxChanged += HandleLeftWheelyxChanged;
		_service.OnRightWheelyxChanged += HandleRightWheelyxChanged;
		_service.OnStateChanged += HandleStateChanged;
	}


	// 비활성화될 때 호출되는 메서드
	void OnDisable()
	{
		// 이벤트 핸들러 해제S
		_service.OnScanResultsChanged -= HandleScanResultsChanged;
		_service.OnSensorAddressChanged -= HandleSensorAddressChanged;
		_service.OnIsScanningChanged -= HandleIsScanningChanged;
		_service.OnLeftWheelyxChanged -= HandleLeftWheelyxChanged;
		_service.OnRightWheelyxChanged -= HandleRightWheelyxChanged;
		_service.OnStateChanged -= HandleStateChanged;
	}

	// 게임 시작 버튼 클릭 시 호출되는 메서드
	public void OnClickStartGame()
	{
        _service.OnScanResultsChanged -= HandleScanResultsChanged;
        _service.OnSensorAddressChanged -= HandleSensorAddressChanged;
        _service.OnIsScanningChanged -= HandleIsScanningChanged;
        _service.OnLeftWheelyxChanged -= HandleLeftWheelyxChanged;
        _service.OnRightWheelyxChanged -= HandleRightWheelyxChanged;
        _service.OnStateChanged -= HandleStateChanged;
        SceneManager.LoadScene(1); // 메인 메뉴 씬 로드
	}

	// 스캔 중지 버튼 클릭 시 호출되는 메서드
	public void OnClickStopScanning()
	{
		_timeout = _startScanDelay; // 타임아웃 설정
		StatusMessage = "Reset"; // 상태 메시지 설정
		_service.Reset(); // 스캔 리셋
        Data.WaitForSeconds(2f); // 2초 대기
		_service.Initialize(); // 블루투스 서비스 초기화
	}

	// 스캔 시작 버튼 클릭 시 호출되는 메서드
	public void OnClickStartScanning()
	{
		leftCumulativeDistance = 0; // 왼쪽 누적 거리 초기화
		rightCumulativeDistance = 0; // 오른쪽 누적 거리 초기화
		StatusMessage = "Scanning ... "; // 상태 메시지 설정
		_startScan = false; // 스캔 시작 플래그 설정
		_timeout = _startScanTimeout; // 타임아웃 설정
		_service.StartScan(); // 스캔 시작
	}

	// 연결 시도 코루틴
	private IEnumerator OnClickConnecting(string deviceAddress)
	{
		StatusMessage = "Connecting..."; // 상태 메시지 설정
		_foundButtonUUID = false; // 버튼 UUID 발견 여부 초기화
		_foundLedUUID = false; // LED UUID 발견 여부 초기화
		_service.StartConnect(deviceAddress); // 기기 주소로 연결 시작
		yield break; // 코루틴 종료
	}

	// 스캔 결과가 변경될 때 호출되는 메서드
	private void HandleScanResultsChanged(object sender, EventArgs _)
	{
		foreach (var kvp in _service.ScanResults) // 스캔 결과를 반복
		{
			Debug.Log("kvp.Value.address" + kvp.Value.Address); // 주소 로그 출력
			if (!_scannedItems.ContainsKey(kvp.Value.Address)) // 스캔된 아이템에 주소가 없으면
			{
				Debug.Log("!_scannedItems.ContainsKey(kvp.Value.address) -> true");
				Debug.Log("kvp.Value.address" + kvp.Value.Address);
				AddToScannedItemsAndButton(kvp.Key, kvp.Value); // 아이템 추가
			}
			else
			{
				Debug.Log("!_scannedItems.ContainsKey(kvp.Value.address) -> false");
			}
		}
	}

	// 센서 주소가 변경될 때 호출되는 메서드
	private void HandleSensorAddressChanged(object sender, EventArgs e)
	{
		StatusMessage = $"Connected to {_service.SensorAddress}"; // 연결된 센서 주소를 상태 메시지에 설정
	}

	// 스캔 상태가 변경될 때 호출되는 메서드
	private void HandleIsScanningChanged(object sender, EventArgs e)
	{
		if (!_service.IsScanning) // 스캔이 진행 중이 아닐 경우
		{
			StatusMessage = "Scan Complete"; // 상태 메시지를 "스캔 완료"로 설정
		}
	}

	// 왼쪽 휠 속도가 변경될 때 호출되는 메서드
	private void HandleLeftWheelyxChanged(object sender, EventArgs e)
	{
		// 왼쪽 휠의 실제 속도를 UI 텍스트에 설정
		LeftWheelSpeedDataText.text = _service.LeftWheelyx.RealSpeed.ToString(CultureInfo.InvariantCulture);
	}

	// 오른쪽 휠 속도가 변경될 때 호출되는 메서드
	private void HandleRightWheelyxChanged(object sender, EventArgs e)
	{
		// 오른쪽 휠의 실제 속도를 UI 텍스트에 설정
		RightWheelSpeedDataText.text = _service.RightWheelyx.RealSpeed.ToString(CultureInfo.InvariantCulture);
	}

	// 블루투스 상태가 변경될 때 호출되는 메서드
	private void HandleStateChanged(object sender, EventArgs e)
	{
		State = _service.State; // 현재 블루투스 상태 업데이트
		if (State == BluetoothState.BleOff) // 블루투스가 꺼져 있을 경우
		{
			StatusMessage = State.ToString(); // 상태 메시지를 현재 상태로 설정
		}
		else
		{
			StatusMessage = $"State changed to: {State}"; // 상태 변경 메시지 설정
		}
	}

	// 스캔된 아이템과 버튼을 추가하는 메서드
	private void AddToScannedItemsAndButton(string key, BleScanResult scanResult)
	{
		GameObject newButton = Instantiate(ButtonPrefab2, Content.transform); // 새로운 버튼 생성
		if (newButton != null) // 버튼이 정상적으로 생성된 경우
		{
			// 생성된 버튼에서 Ble_ScannedItemScript 컴포넌트를 가져옴
			if (newButton.TryGetComponent<Ble_ScannedItemScript>(out var scannedItem))
			{
				var buttonScript = newButton.GetComponent<Ble_ScannedItemScript>(); // 스크립트 가져오기
				buttonScript.setNameValue(scanResult.Name); // 기기 이름 설정
				buttonScript.setAddressValue(scanResult.Address); // 기기 주소 설정
				buttonScript.setRssiValue(scanResult.Rssi); // RSSI 값 설정
				_scannedItems.Add(scanResult.Address, buttonScript); // 딕셔너리에 아이템 추가

				// 버튼 이름 설정
				newButton.name = "Button_" + scanResult.Name;

				Button buttonComponent = newButton.GetComponent<Button>(); // 버튼 컴포넌트 가져오기
				if (buttonComponent != null) // 버튼 컴포넌트가 정상적으로 가져와졌다면
				{
					// 버튼 클릭 시 연결 시도하는 코루틴을 실행
					buttonComponent.onClick.AddListener(() => StartCoroutine(OnClickConnecting(scanResult.Address)));
				}

			}
		}
	}
	public void OnChangeControlMethod()
	{
		Data.IsWheelyXControlling = WheelyXToggle.isOn;

    }
}
