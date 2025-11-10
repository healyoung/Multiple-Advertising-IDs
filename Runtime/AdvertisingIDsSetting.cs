using System;
using System.Collections.Generic;
using HealYoung;
using UnityEngine;

namespace WhiteEngine
{
    [CreateAssetMenu(fileName = "AdvertisingIDsSettings", menuName = "AdvertisingIDsSettings")]
    public class AdvertisingIDsSettings : ScriptableObject
    {
        public List<string> bannerIds = new List<string>();
        public List<string> interstitialIds = new List<string>();
        public readonly LoginDayCounter DayCounter = new LoginDayCounter();

        private List<string> GetIDsByDay(AdvertingType advertingType)
        {
            List<string> result = new List<string>();
            int index = DayCounter.LoginDay.Value - 1;
            index = Mathf.Clamp(index, 0, 3);
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

    public enum AdvertingType
    {
        Banner,
        Interstitial
    }
}