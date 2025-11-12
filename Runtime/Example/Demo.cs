using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WhiteEngine;

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
        currentDay.text = $"第{MultipleAdIds.AdvertisingIDs.DayCounter.LoginDay.Value}天";
        var bannerId = MultipleAdIds.AdvertisingIDs.GetAdvertisingID(AdvertingType.Banner);
        currentBanner.text = bannerId;
        
        var interstitialId = MultipleAdIds.AdvertisingIDs.GetAdvertisingID(AdvertingType.Interstitial);
        currentInterstitial.text = interstitialId;
    }


    private void NextDay()
    {
        MultipleAdIds.AdvertisingIDs.DayCounter.LoginDay.Value++;
        MultipleAdIds.AdvertisingIDs.IntervalMTimer.Value = 0;
        MultipleAdIds.AdvertisingIDs.IntervalNTimer.Value = 0;
        currentDay.text = $"第{MultipleAdIds.AdvertisingIDs.DayCounter.LoginDay.Value}天";
        var bannerId = MultipleAdIds.AdvertisingIDs.GetAdvertisingID(AdvertingType.Banner);
        currentBanner.text = bannerId;
        
        var interstitialId = MultipleAdIds.AdvertisingIDs.GetAdvertisingID(AdvertingType.Interstitial);
        currentInterstitial.text = interstitialId;
    }
    
    private void NextBanner()
    {
        var bannerId = MultipleAdIds.AdvertisingIDs.GetAdvertisingID(AdvertingType.Banner);
        currentBanner.text = bannerId;
    }
    
    private void NextInterstitial()
    {
        var interstitialId = MultipleAdIds.AdvertisingIDs.GetAdvertisingID(AdvertingType.Interstitial);
        currentInterstitial.text = interstitialId;
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