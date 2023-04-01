 using UnityEngine;
using System.Collections;

public class WebcamMovement : MonoBehaviour
{

    public int sensitivity = 50; // 모션 감도 설정
    public int width = 320; // 웹캠 너비 설정
    public int height = 240; // 웹캠 높이 설정

    private WebCamTexture webcamTexture; // 웹캠 텍스처
    private Color32[] previousFrame; // 이전 프레임

    void Start()
    {
        webcamTexture = new WebCamTexture(width, height); // 웹캠 텍스처 초기화 및 크기 설정
        previousFrame = new Color32[webcamTexture.width * webcamTexture.height]; // 이전 프레임 배열 초기화
        GetComponent<Renderer>().material.mainTexture = webcamTexture; // 웹캠 텍스처를 머티리얼의 메인 텍스처로 설정
        webcamTexture.Play(); // 웹캠 시작
    }

    void Update()
    {
        if (webcamTexture.didUpdateThisFrame) // 웹캠이 업데이트 되었을 때
        {
            Color32[] pixels = webcamTexture.GetPixels32(); // 현재 프레임 픽셀 정보 가져오기
            int changedPixels = 0; // 변화된 픽셀 수 초기화
            for (int i = 0; i < pixels.Length; i++) // 모든 픽셀에 대해서
            {
                if (i < previousFrame.Length && DifferentPixels(previousFrame[i], pixels[i]) > sensitivity) // 이전 프레임과 현재 프레임의 픽셀 값 차이가 모션 감도보다 크면
                {
                    changedPixels++; // 변화된 픽셀 수 증가
                }
            }
            if (changedPixels > (pixels.Length * 0.01f)) // 변화된 픽셀 수가 전체 픽셀 수의 1% 이상이면 (모션 감지 시)
            {
                Debug.Log("Motion Detected!"); // "Motion Detected!" 로그 출력
            }
            previousFrame = pixels; // 현재 프레임을 이전 프레임으로 저장
        }
    }

    int DifferentPixels(Color32 a, Color32 b) // 두 개의 Color32 값을 받아서 차이를 반환하는 함수
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
    }
}