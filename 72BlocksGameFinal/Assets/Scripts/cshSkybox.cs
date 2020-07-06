using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class cshSkybox : MonoBehaviourPun
{
    public Material day;
    public Material sunset;
    public Material night;
    public Material rain;
   
    public int t;
    public float passTime = 0.0f;
    public float changeTime = 10.0f;
    public float changeValue = 10.0f;

    public Light keyLight;
    public float dayIntensity = 1;
    public Color32 dayFog;
    public float sunsetIntensity = 1;
    public Color32 sunsetFog;
    public float nightIntensity = 1;
    public Color32 nightFog;
    public float rainIntensity = 1;
    public Color32 rainFog;

    public Color32 currentfogColor;
    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
            return;
        RenderSettings.fog = true;
        currentfogColor = RenderSettings.fogColor;
        //RenderSettings.skybox = day;
        t = 1;
    }
    [PunRPC]
    public void setTimeValue(int id, int tval, float pTime)
    {
        cshSkybox target = PhotonView.Find(id).GetComponent<cshSkybox>();
        target.t = tval;
        target.passTime = pTime;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!photonView.IsMine)
        //    return;
        if (photonView.IsMine)
        {
            GameObject manager = GameObject.Find("GameManager");
            photonView.RPC("setTimeValue", RpcTarget.All, manager.GetPhotonView().ViewID, t, passTime);
        }


        passTime += Time.deltaTime;

        switch (t)
        {
            case 1:
               // RenderSettings.skybox = day;
                if (RenderSettings.skybox != day)
                {
                    if (Mathf.Abs(dayIntensity - keyLight.intensity) > 0.01f)
                    {
                        keyLight.intensity = Mathf.Lerp(keyLight.intensity, dayIntensity, Time.deltaTime * changeValue);
                        RenderSettings.fogColor = dayFog;
                    }
                    else
                    {
                        RenderSettings.skybox = day;
                        currentfogColor = dayFog;
                       
                    }
                }
                break;
            case 2:
              //  RenderSettings.skybox = sunset;
                if (RenderSettings.skybox != sunset)
                {
                    if (Mathf.Abs(sunsetIntensity - keyLight.intensity) > 0.01f)
                    {
                        keyLight.intensity = Mathf.Lerp(keyLight.intensity, sunsetIntensity, Time.deltaTime * changeValue);
                        RenderSettings.fogColor = sunsetFog;
                    }
                    else
                    {
                        RenderSettings.skybox = sunset;
                        currentfogColor = sunsetFog;
                       
                    }
                }
                break;
            case 3:
              //  RenderSettings.skybox = night;
                if (RenderSettings.skybox != night)
                {
                    if (Mathf.Abs(nightIntensity - keyLight.intensity) > 0.01f)
                    {
                        keyLight.intensity = Mathf.Lerp(keyLight.intensity, nightIntensity, Time.deltaTime * changeValue);
                        RenderSettings.fogColor = nightFog;
                    }
                    else
                    {
                        RenderSettings.skybox = night;
                        currentfogColor = nightFog;
                       
                    }
                }
                break;
            case 4:
                //RenderSettings.skybox = rain;
                if (RenderSettings.skybox != rain)
                {
                    if (Mathf.Abs(rainIntensity - keyLight.intensity) > 0.01f)
                    {
                        keyLight.intensity = Mathf.Lerp(keyLight.intensity, rainIntensity, Time.deltaTime * changeValue);
                        RenderSettings.fogColor = rainFog;
                    }
                    else
                    {
                        RenderSettings.skybox = rain;
                        currentfogColor = rainFog;
                        
                    }
                }
                break;

        }

        if(passTime > changeTime)
        {
            t++;
            passTime = 0.0f;
        }

        if(t > 4)
        {
            t = 1;
        }
    }
}
