using System;
using System.Collections.Generic;
using HealYoung;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace WhiteEngine
{
    public class AdvertisingIDsSettings : ScriptableObject
    {
        
        [Header("要求云控使用的每日横幅广告ID的间隔")][SerializeField, ReadOnly] public string remoteBannerInterval = "remote_banner_interval";
        [Header("要求云控使用的每日插屏广告ID的间隔")] [SerializeField, ReadOnly] public string remoteInterstitialInterval = "remote_interstitial_interval";
        [Header("要求云控使用的所有插屏广告ID")] [SerializeField, ReadOnly] public string allInterstitialIds = "all_interstitial_ids";
        [Header("要求云控使用的所有横幅广告ID")] [SerializeField, ReadOnly] public string allBannerIds = "all_banner_ids";
        
        [Header("横幅广告ID")] [SerializeField] public List<string> bannerIds = new List<string>();
        
        [Header("插屏广告ID")] [SerializeField] public List<string> interstitialIds = new List<string>();
        
        public readonly LoginDayCounter DayCounter = new LoginDayCounter();

        private List<string> GetIDsByDay(AdvertingType advertingType)
        {
            List<string> result = new List<string>();
            int index = DayCounter.LoginDay.Value;
            index = Mathf.Clamp(index, 1, 4);
            int start = index * 2 - 2;
            int end = index * 2;
            switch (advertingType)
            {
                case AdvertingType.Banner:
                    for (var i = start; i < end; i++)
                    {
                        result.Add(bannerIds[i]);
                    }

                    break;
                case AdvertingType.Interstitial:
                    for (var i = start; i < end; i++)
                    {
                        result.Add(interstitialIds[i]);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(advertingType), advertingType, null);
            }

            return result;
        }

        public string GetAdvertisingID(AdvertingType advertingType)
        {
            int index = 0;
            switch (advertingType)
            {
                case AdvertingType.Banner:
                    index = IntervalNTimer.Value >= IntervalN.Value ? 1 : 0;
                    IntervalNTimer.Value++;
                    break;
                case AdvertingType.Interstitial:
                    index = IntervalMTimer.Value >= IntervalM.Value ? 1 : 0;
                    IntervalMTimer.Value++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(advertingType), advertingType, null);
            }

            List<string> ids = GetIDsByDay(advertingType);
            return ids[index];
        }

        public IBindableProperty<int> IntervalN;

        public IBindableProperty<int> IntervalM;

        public IBindableProperty<int> IntervalNTimer;

        public IBindableProperty<int> IntervalMTimer;
    }

    public class Day
    {
        public string BannerId;
        public string InterstitialId;
    }

    public enum AdvertingType
    {
        Banner,
        Interstitial
    }
}