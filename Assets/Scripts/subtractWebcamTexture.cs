using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class subtractWebcamTexture : MonoBehaviour
{
    #region 웹캠으로 부터 텍스쳐 입력을 받아오기
    [Header("연결된 카메라중 몇 번째 카메라를 사용할 것인(디폴=0)")]
    public int DeviceIndex;

    [Header("사용할 카메라의 해상도")]
    //해상도에 따라 연산방식을 변결할 필요가 있음
    public int width = 128;
    public int height = 128;
    public int fps = 30;

    WebCamTexture webcamTexture;

    #endregion


    //현재 프레임과 이전 프레인의 차이를 저장할 렌더 텍스쳐
    public RenderTexture frame01;
    Texture2D frame01Tex2D;

    public RenderTexture frame02;
    Texture2D frame02Tex2D;

    [Header("결과 텍스쳐를 받아옵니다")]
    public RenderTexture resultRenderTexture;
    Texture2D resultRenderTex2D;

    Color[] results;

    [Range(0f, 1f)]
    public float threshold;


    void SetWebCamTexture(int index)
    {
        if(webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(devices[index].name, this.width, this.height, this.fps);
        webcamTexture.Play();
    }
    private void Start()
    {
        SetWebCamTexture(DeviceIndex);

        //렌더 텍스쳐를 Texture2D로 변환, 픽셀연산을 진행하기 위해 생서
        frame01Tex2D = new Texture2D(frame01.width, frame01.height);
        //결과값을 저장하기 위해서
        resultRenderTex2D = new Texture2D(frame02.width, frame02.height);
        results = new Color[width * height];

        //무한 코루틴을 이용해, 시간차가 있는 웹캠 텍스쳐를 실시간으로 받아온.
        StartCoroutine("GetFrameTexture");
    }
    IEnumerator GetFrameTexture()
    {
        while(true)
        {
            //앞선 프레임의 웹캠의 텍스쳐를 받아온다.
            Graphics.Blit(webcamTexture, frame01);
            frame01Tex2D = toTexture2D(frame01);
            Color[] frame01Tex2DColors = new Color[width * height];
            frame01Tex2DColors = frame01Tex2D.GetPixels();

            //약 한 프레임 정도 쉬고
            yield return new WaitForSeconds(0.03f);

            //다음 프레임의 웹캠의 텍스쳐를 받아온다.
            Graphics.Blit(webcamTexture, frame02);
            frame02Tex2D = toTexture2D(frame02);
            Color[] frame02Tex2DColors = new Color[width * height];
            frame02Tex2DColors = frame02Tex2D.GetPixels();

            #region 01.차연산을 바로 텍스쳐에 적용해 줄 때

            for (int i = 0; i < results.Length; i++)
            {
                //차영상
                Color SubColor = frame01Tex2DColors[i] - frame02Tex2DColors[i];

                //그레이 스케일 변환
                float sum = SubColor.r + SubColor.g + SubColor.b;
                SubColor.r = sum / 3;
                SubColor.g = sum / 3;
                SubColor.b = sum / 3;
                SubColor.a = 1;

                //이진화
                if (SubColor.maxColorComponent < threshold)
                {
                    SubColor = Color.black;
                }
                else
                {
                    SubColor = Color.white;
                }
                //보간을 통해 변화율을 늦춰준다.
                results[i] = Color.Lerp(results[i], SubColor, 1.0f - Mathf.Exp(-7 * Time.deltaTime));
            }

            #endregion

            #region 02.부드러운 변화를 이용해 보간 효과를 넣어줄 때(값이 작기 때문에 vfxgrapah에서 값을 키워줄 필요가 있다)

            for (int i = 0; i < results.Length; i++)
            {
                //차영상
                Color SubColor = frame01Tex2DColors[i] - frame02Tex2DColors[i];

                //그레이 스케일 변환
                float sum = SubColor.r + SubColor.g + SubColor.b;
                SubColor.r = sum / 3;
                SubColor.g = sum / 3;
                SubColor.b = sum / 3;
                SubColor.a = 1;

                //이진화
                if (SubColor.maxColorComponent < threshold)
                {
                    SubColor = Color.black;
                }
                else
                {
                    SubColor = Color.white;
                }
                //보간을 통해 변화율을 늦춰준다.
                results[i] = Color.Lerp(results[i], SubColor, 1.0f - Mathf.Exp(-7 * Time.deltaTime));
            }

            #endregion

            resultRenderTex2D.SetPixels(results);
            resultRenderTex2D.Apply();//유의
            RenderTexture.active = resultRenderTexture;
            Graphics.Blit(resultRenderTex2D, resultRenderTexture);
            yield return null;
        }
    }
    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D resultTexture = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);

        RenderTexture.active = rTex;
        resultTexture.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);

        //변경된 데이터를 적용 시킨다.
        resultTexture.Apply();
        return resultTexture;
    }

   
}