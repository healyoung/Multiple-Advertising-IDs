using System;
using System.Collections;
using System.Collections.Generic;
using HealYoung;
using ShanHai;
using UnityEngine;

public class LoginDayCounter
{
    public IBindableProperty<int> LastLoginTime;

    public IBindableProperty<int> LoginDay;
    
    
    public void UpdateLastLoginTime()
    {
        var lastLoginTime = ConvertTimeStampToDateTime(LastLoginTime.Value);
        if (IsOneDayLaterWithDifference(lastLoginTime, out _))
        {
            LoginDay.Value++;
        }

        Debug.Log($"Login Day is {LoginDay.Value}");
        LastLoginTime.Value = ConvertDateTimeToTimeStamp(DateTime.Now);
    }
    
    
    /// <summary>
    /// 判断当前时间是否比传入时间大一天，并返回实际相差的天数
    /// </summary>
    /// <param name="targetDateTime">要比较的目标时间</param>
    /// <param name="daysDifference">返回实际相差的天数</param>
    /// <returns>如果当前日期比目标日期大至少一天，返回true</returns>
    private static bool IsOneDayLaterWithDifference(DateTime targetDateTime, out int daysDifference)
    {
        DateTime currentDate = DateTime.Today;
        DateTime targetDate = targetDateTime.Date;
        
        TimeSpan difference = currentDate - targetDate;
        daysDifference = difference.Days;
        
        return daysDifference >= 1;
    }
    
    /// <summary>
    /// 时间转时间戳
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static int ConvertDateTimeToTimeStamp(DateTime time)
    {
        string id = TimeZoneInfo.Local.Id;
        DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        DateTime startTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(id));
        return (int)(time - startTime).TotalSeconds;
    }


    /// <summary>
    /// 时间戳转时间
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public static DateTime ConvertTimeStampToDateTime(int timeStamp)
    {
        string id = TimeZoneInfo.Local.Id;
        DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        DateTime dtStart = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(id));
        long lTime = long.Parse(timeStamp + "0000000");
        TimeSpan toNow = new TimeSpan(lTime);
        return dtStart.Add(toNow);
    }
}
