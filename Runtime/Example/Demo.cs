using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ShanHai;

public class Demo : MonoBehaviour
{
    private void OnEnable()
    {
        nextDay.onClick.AddListener(NextDay);
        nextBanner.onClick.AddListener(NextBanner);
        nextInterstitial.onClick.AddListener(NextInterstitial);
        clearData.onClick.AddListener(ClearData);
    }

    private void OnDisable()
    {
        nextDay.onClick.RemoveListener(NextDay);
        nextBanner.onClick.RemoveListener(NextBanner);
        nextInterstitial.onClick.RemoveListener(NextInterstitial);
        clearData.onClick.RemoveListener(ClearData);
    }

    private void Start()
    {
        currentDay.text = $"第{MultipleAdIds.DayCounter.LoginDay.Value}天";
        var bannerId = MultipleAdIds.GetAdvertisingID(AdvertingType.Banner);
        currentBanner.text = bannerId;
        
        var interstitialId = MultipleAdIds.GetAdvertisingID(AdvertingType.Interstitial);
        currentInterstitial.text = interstitialId;
    }


    private void NextDay()
    {
        MultipleAdIds.DayCounter.LoginDay.Value++;
        MultipleAdIds.OnNewDay();
        currentDay.text = $"第{MultipleAdIds.DayCounter.LoginDay.Value}天";
        var bannerId = MultipleAdIds.GetAdvertisingID(AdvertingType.Banner);
        currentBanner.text = bannerId;
        
        var interstitialId = MultipleAdIds.GetAdvertisingID(AdvertingType.Interstitial);
        currentInterstitial.text = interstitialId;
    }
    
    private void NextBanner()
    {
        var bannerId = MultipleAdIds.GetAdvertisingID(AdvertingType.Banner);
        currentBanner.text = bannerId;
        MultipleAdIds.ShowAdSucceed(AdvertingType.Banner);
    }
    
    private void NextInterstitial()
    {
        var interstitialId = MultipleAdIds.GetAdvertisingID(AdvertingType.Interstitial);
        currentInterstitial.text = interstitialId;
        MultipleAdIds.ShowAdSucceed(AdvertingType.Interstitial);
    }
    
    private void ClearData()
    {
        PlayerPrefs.DeleteAll();
    }

    [SerializeField] private Button nextDay;
    [SerializeField] private Button nextBanner;
    [SerializeField] private Button nextInterstitial;
    [SerializeField] private Button clearData;
    [SerializeField] private Text currentDay;
    [SerializeField] private Text currentInterstitial;
    [SerializeField] private Text currentBanner;
}
