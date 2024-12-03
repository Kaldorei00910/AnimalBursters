# AnimalBursters
- 휠체어로 움직임을 제어 가능한 로그라이크 게임
- 레퍼런스 : 탕탕특공대, RiskOfRain2, VampireSurvivers 등

 ### 개발환경 
 - Unity2022.3.45f1
 - Microsoft Visual Studio Community 2022 버전 17.11.5
 - WheelyHub

## 시연영상
- 블루투스 연결

https://github.com/user-attachments/assets/97b7f5ef-9096-4a63-995c-041511880426

- 실제 플레이 영상

[![시연 영상](https://github.com/user-attachments/assets/d624fca7-9cbd-49b5-ba0e-636de7457b8c)](https://vimeo.com/1035473770 "클릭하여 재생")

## 구현기능 요약
- [블루투스 연결을 통해 외부 기기와의 연결](#bluetooth-연결)
- [Event를 통해 구현한 플레이어,몬스터 이동 동작](#event를-통해-구현한-플레이어-몬스터-이동-동작)

- WheelyXController를 이용한 외부 신호 -> 플레이어 동작 연결
- Cinemachine을 통한 시작 시 카메라 연출 구현
- 몬스터 동작, 행동패턴 구현
- 보스몬스터 패턴 구현


## bluetooth 연결
- 사용 에셋
[![사용 에셋](https://github.com/user-attachments/assets/6f75fe5a-ef60-421b-92dc-7c2fe8a2af73)](https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661?locale=ko-KR&srsltid=AfmBOoqmvJw_I5McSxj5iPVLKb7B2ieTsmJZ5pePY90ItfN9AJt-AbQC "Bluetooth LE for iOS, tvOS and Android")
- 블루투스 UUID값을 통해 주변 기기 탐색 및 비교
- 발견된 아이템을 정보화 함께 버튼으로 생성
- 클릭 시 연결 시도 (ConnectCoroutine), 성공 시 구독까지 진행
- Ble_WheelyHubData 클래스를 통해 데이터 수신

## event를 통해 구현한 플레이어 몬스터 이동 동작
- BaseController의 Action OnMoveEvent에 필요한 메서드를 구독하여 동작

