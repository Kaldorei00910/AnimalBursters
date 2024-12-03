using System; // 기본적인 시스템 기능을 위한 네임스페이스
using System.Collections; // 컬렉션 관련 기능을 위한 네임스페이스
using System.Collections.Generic; // 제네릭 컬렉션을 사용하기 위한 네임스페이스
using Unity.VisualScripting; // Visual Scripting 관련 네임스페이스 (필요한 경우)
using UnityEngine; // UnityEngine 네임스페이스를 임포트합니다.
using UnityEngine.UI; // UI 요소를 위한 UnityEngine.UI 네임스페이스를 임포트합니다;

// 블루투스 상태를 나타내는 열거형
public enum BluetoothState // 수정된 부분: public으로 변경
{
    Initialize, // 초기화 상태
    Scanning, // 스캔 중
    ScanComplete, // 스캔 완료
    Connecting, // 연결 중
    Connected, // 연결됨
    Subscribing, // 구독 중
    FailConnect, // 연결 실패
    Disconnected, // 연결 끊김
    Reset, // 리셋 상태
    Error, // 오류 상태
    BleOff, // Bluetooth 꺼짐
    End // 종료 상태
}

// 블루투스 서비스를 처리하는 클래스
public class BluetoothService : MonoBehaviour
{
    // 서비스 및 특성 UUIDs
    static string[] ServiceUUIDs = new string[] { "########-####-####-####-############" };
    static string ServiceUUID = "########-####-####-####-############";
    static string wheelyxSpeedCharacteristicUUID = "########-####-####-####-############";

    // 싱글톤 패턴을 위한 인스턴스
    private static BluetoothService _instance;
    public static BluetoothService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BluetoothService>();
                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(BluetoothService).ToString());
                    _instance = singleton.AddComponent<BluetoothService>();
                    DontDestroyOnLoad(singleton); // 씬 전환 시에도 파괴되지 않도록 설정
                }
            }
            return _instance; // 인스턴스 반환
        }
    }

    private Dictionary<string, BleScanResult> _scanResults; // 스캔 결과를 저장할 딕셔너리
    public Dictionary<string, BleScanResult> ScanResults
    {
        get => _scanResults;
        private set
        {
            Debug.Log("OnScanResultsChanged 호출");
            _scanResults = value;
        }
    }

    // 스캔 결과를 설정하는 내부 메서드
    private void _setScanResult(string name, BleScanResult value)
    {
        if (!ScanResults.ContainsKey(value.Address)) // 이미 존재하지 않는 경우만 추가
        {
            Debug.Log("SetScanResult 호출");
            ScanResults[name] = value; // 스캔 결과 추가
            Debug.Log(value.Address + "가 추가됨");
            OnScanResultsChanged?.Invoke(this, EventArgs.Empty); // 이벤트 호출
        }
    }

    // 스캔 결과 변경 시 발생하는 이벤트
    public event EventHandler OnScanResultsChanged;

    private string sensorAddress; // 센서 주소
    public string SensorAddress
    {
        get => sensorAddress;
        private set
        {
            sensorAddress = value; // 주소 설정
            OnSensorAddressChanged?.Invoke(this, EventArgs.Empty); // 이벤트 호출
        }
    }
    public event EventHandler OnSensorAddressChanged; // 센서 주소 변경 시 이벤트

    private bool isScanning; // 스캔 상태
    public bool IsScanning
    {
        get => isScanning;
        private set
        {
            isScanning = value; // 스캔 상태 설정
            OnIsScanningChanged?.Invoke(this, EventArgs.Empty); // 이벤트 호출
        }
    }
    public event EventHandler OnIsScanningChanged; // 스캔 상태 변경 시 이벤트

    private Ble_WheelyHubSpeedVO leftWheelyx; // 왼쪽 휠 속도 데이터
    public Ble_WheelyHubSpeedVO LeftWheelyx
    {
        get => leftWheelyx;
        private set
        {
            leftWheelyx = value; // 왼쪽 휠 속도 설정
            OnLeftWheelyxChanged?.Invoke(this, EventArgs.Empty); // 이벤트 호출
        }
    }
    public event EventHandler OnLeftWheelyxChanged; // 왼쪽 휠 속도 변경 시 이벤트

    private Ble_WheelyHubSpeedVO rightWheelyx; // 오른쪽 휠 속도 데이터
    public Ble_WheelyHubSpeedVO RightWheelyx
    {
        get => rightWheelyx;
        private set
        {
            rightWheelyx = value; // 오른쪽 휠 속도 설정
            OnRightWheelyxChanged?.Invoke(this, EventArgs.Empty); // 이벤트 호출
        }
    }
    public event EventHandler OnRightWheelyxChanged; // 오른쪽 휠 속도 변경 시 이벤트

    private BluetoothState _state; // 블루투스 상태
    public BluetoothState State
    {
        get => _state;
        private set
        {
            _state = value; // 상태 설정
            OnStateChanged?.Invoke(this, EventArgs.Empty); // 이벤트 호출
        }
    }
    public event EventHandler OnStateChanged; // 상태 변경 시 이벤트

    // 이전 및 현재 웨일리 허브 데이터
    public static Ble_WheelyHubData previous { get; set; } = new Ble_WheelyHubData(0, 0, 0, 0);
    public static Ble_WheelyHubData current { get; private set; } = new Ble_WheelyHubData(0, 0, 0, 0);

    // Awake 메서드: 초기화 작업
    public void Awake()
    {
        ScanResults = new Dictionary<string, BleScanResult>(); // 스캔 결과 초기화
        Initialize(); // 블루투스 초기화
    }

    // 블루투스 초기화 메서드
    public void Initialize()
    {
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            SetState(BluetoothState.Initialize); // 상태를 초기화로 설정
        },
        (error) =>
        {
            SetState(BluetoothState.Error); // 오류 발생 시 상태를 오류로 설정
            if (error == "Bluetooth LE Not Enabled")
            {
                BluetoothLEHardwareInterface.Log("error Check!!!");
                SetState(BluetoothState.BleOff); // Bluetooth가 꺼진 경우 상태를 꺼짐으로 설정
            }

            BluetoothLEHardwareInterface.Log("Error: " + error + " error Check!!!"); // 오류 로그 출력
        });
    }

    // 스캔 시작 메서드
    public void StartScan()
    {
        SetState(BluetoothState.Scanning); // 상태를 스캔 중으로 설정
        IsScanning = true; // 스캔 상태 설정
        BluetoothLEHardwareInterface.Log("StartScan() 진입");

        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
             ServiceUUIDs, null, (address, name, rssi, bytes) =>
             {
                 BluetoothLEHardwareInterface.Log("item scanned: " + address); // 스캔된 아이템 로그 출력

                 if (ScanResults.ContainsKey(address)) // 이미 존재하는 경우
                 {
                     BluetoothLEHardwareInterface.Log("scanResults.ContainsKey(address) -> true");
                     var scannedItem = ScanResults[address]; // 스캔 결과 가져오기
                     scannedItem.setRssi(rssi.ToString()); // RSSI 업데이트
                     _setScanResult(address, scannedItem); // 스캔 결과 설정
                 }
                 else // 새로 발견된 경우
                 {
                     BluetoothLEHardwareInterface.Log("scanResults.ContainsKey(address) -> false");
                     BleScanResult scannedItem = new(name, address, rssi.ToString()); // 새로운 스캔 아이템 생성
                     _setScanResult(address, scannedItem); // 스캔 결과 설정
                     BluetoothLEHardwareInterface.Log("Adding new device to ScanResults: " + address); // 추가 로그 출력
                 }
                 Debug.Log("스캔1회");
             }, true); // 스캔 시작
    }

    public void StopScan()
    {
        IsScanning = false; // 스캔 상태 설정
        SetState(BluetoothState.ScanComplete); // 상태를 스캔 완료로 설정
        BluetoothLEHardwareInterface.StopScan(); // 스캔 중지
    }

    // 연결 시작 메서드
    public void StartConnect(string deviceAddress)
    {
        StartCoroutine(ConnectCoroutine(deviceAddress)); // 연결 코루틴 시작
    }
#pragma warning disable IDE0052, CS0414
    private bool isConnectedDevice = false; // 연결된 디바이스 여부
    //이하 람다식에서 사용하는 변수, 오류표시 제거
#pragma warning restore IDE0052, CS0414

    // 연결 코루틴: 주어진 기기에 연결을 시도하는 메서드
    private IEnumerator ConnectCoroutine(string deviceAddress)
    {
        int epoch = 0; // 연결 시도 횟수
        int max_epoch = 3; // 최대 연결 시도 횟수

        // 최대 연결 시도 횟수에 도달할 때까지 반복
        while (epoch < max_epoch)
        {
            bool connectionAttempted = false; // 연결 시도 여부

            BluetoothLEHardwareInterface.ConnectToPeripheral(
                deviceAddress,
                (_) =>
                {
                    SetState(BluetoothState.Connecting); // 상태를 연결 중으로 변경
                    connectionAttempted = true; // 연결 시도 완료
                },
                null, // 서비스 작업은 사용하지 않음
                (address, serviceUUID, characteristicUUID) =>
                { // 연결 성공 시 호출되는 Action
                    SetState(BluetoothState.Connected); // 상태를 연결됨으로 변경
                    SensorAddress = deviceAddress; // 센서 주소 설정
                    isConnectedDevice = true; // 연결된 디바이스 플래그 설정
                    BluetoothLEHardwareInterface.StopScan(); // 스캔 중지
                },
                (_) =>
                { // 연결 실패 시 호출되는 Action
                    epoch++; // 시도 횟수 증가
                    SetState(BluetoothState.FailConnect); // 상태를 연결 실패로 변경
                    SensorAddress = null; // 센서 주소 초기화
                    isConnectedDevice = false; // 연결된 디바이스 플래그 초기화
                    connectionAttempted = true; // 연결 시도 완료
                }
            );

            // 연결 시도 완료될 때까지 대기
            yield return new WaitUntil(() => connectionAttempted);

            // 연결 성공 시 코루틴 종료
            if (SensorAddress != null)
            {
                StartCoroutine(SubscirbeCoroutine()); // 구독 코루틴 시작
                yield break; // 코루틴 종료
            }

            // 연결 시도 간 대기 시간 설정 (옵션)
            yield return Data.WaitForSeconds(1f); // 1초 대기
        }
        // todo 최대 시도 횟수에 도달한 경우 처리
        // ShowMaxConnectTryError
    }

    // 구독 코루틴: 센서 데이터 구독을 설정하는 메서드
    private IEnumerator SubscirbeCoroutine()
    {
        yield return Data.WaitForSeconds(2f); // 2초 대기

        // 센서 주소, 서비스 UUID 및 특성 UUID로 구독 설정
        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(SensorAddress, ServiceUUID, wheelyxSpeedCharacteristicUUID,
            (notifyAddress, notifyCharacteristic) =>
            {
                Data.WaitForSeconds(2f); // 2초 대기
                                        // 센서에서 특성을 읽음
                BluetoothLEHardwareInterface.ReadCharacteristic(SensorAddress, ServiceUUID, wheelyxSpeedCharacteristicUUID, (characteristic, bytes) =>
                {
                    SetState(BluetoothState.Subscribing); // 상태를 구독 중으로 변경
                });
            },
            (address, wheelyxSpeedCharacteristicUUID, bytes) =>
            {
                // 읽은 데이터를 사용하여 웨일리 허브 데이터 업데이트
                UpdateWheelyHubData(Ble_WheelyHubData.FromHex(bytes));
            });
    }

    // 모든 연결을 끊는 메서드
    public void DisconnectAll()
    {
        // todo: DisconnectAll 되었는지 확인 필요
        BluetoothLEHardwareInterface.DisconnectAll(); // 모든 연결 끊기
        SetState(BluetoothState.Disconnected); // 상태를 연결 끊김으로 변경
    }

    // 스캔 결과 초기화 및 리셋 메서드
    public void Reset()
    {
        ScanResults.Clear(); // 스캔 결과 초기화
        StartCoroutine(ResetCoroutine()); // 리셋 코루틴 시작
    }

    // 상태 설정 메서드
    private void SetState(BluetoothState newState)
    {
        State = newState; // 상태 변경
    }

    // 리셋 코루틴: 리셋 관련 작업을 수행하는 메서드
    private IEnumerator ResetCoroutine()
    {
        SetState(BluetoothState.Reset); // 상태를 리셋으로 설정
        DisconnectAll(); // 모든 연결 끊기
        StopScan(); // 스캔 중지
        yield break; // 코루틴 종료
    }

    // 휠리 허브 데이터를 업데이트하는 메서드
    public void UpdateWheelyHubData(Ble_WheelyHubData calCurrent)
    {
        current = calCurrent; // 현재 데이터 업데이트
                              // 왼쪽 및 오른쪽 휠 속도 모델 생성
        LeftWheelyx = Ble_WheelyHubSpeedVO.FromRawLeftWheelyxModel(previous, current);
        RightWheelyx = Ble_WheelyHubSpeedVO.FromRawRightWheelyxModel(previous, current);

        // 이전 데이터 업데이트
        previous.Battery = current.Battery;
        previous.LeftWheelRev = current.LeftWheelRev;
        previous.RightWheelRev = current.RightWheelRev;
        previous.MeasureTime = current.MeasureTime;
    }

    // 테스트 허브 데이터를 처리하는 메서드
    public void HandleTestHubDate(Ble_WheelyHubSpeedVO value)
    {
        LeftWheelyx = value; // 왼쪽 휠 속도 설정
        RightWheelyx = value; // 오른쪽 휠 속도 설정
    }
}
