using System.Globalization;
using gui = UnityEngine.GUILayout;
#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Prefs = UnityEngine.PlayerPrefs;
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;



[Serializable]
public class Posts
{
    public string title;
    public string msg;
    public DateTime date;
    public int comments;
    public string imageUrl;
    public Texture2D image;
    public string id;

    public IEnumerator Load()
    {
        if(!string.IsNullOrEmpty(imageUrl))
        {
            WWW w = new WWW(imageUrl);
            yield return w;
            image = w.texture;
        }
    }
        
}
public partial class Integration
{
    public bool vkLoggedIn;

    //public void VkLogin()
    //{
    //    vkLoggedIn = true;
    //    print("VkLogin");        
    //}
    public void StartVk()
    {
        //if (isDebug)
        //{
        //    VkInfo2("212511867,Raimo Nieminen,http://cs303603.vk.me/v303603867/4b03/zh7qvX_z6ls.jpg");
        //    VkNews("Все качайте игру на телефон,мы сделали видео обзор.;;;0;;;1388879381;;;4692;;;http://cs617131.vk.me/u189810914/video/l_1d2ec6c0.jpg###Набор в тестеры снова открыт http://vk.com/topic-59755500_29093610?offset=20, только помни надо быть активным!;;;0;;;1388851186;;;4674;;;###Дорогие гонщики,мы вам всем дали по джипам,у модераторов теперь новая машина.;;;12;;;1388759908;;;4620;;;###Обновление в силе!;;;3;;;1388753097;;;4601;;;http://cs310228.vk.me/v310228694/780e/P-TEAv8s54c.jpg###Скоро будут тесты новой версии, кто хочет сыграть первым записываемся в тестера!;;;32;;;1388601988;;;4547;;;###Как вы думаете когда 500.000 гонщиков будет ?;;;0;;;1388578161;;;4534;;;###Дорогие Гонщики!Поздравляем вас с Новым годом!Желаем вам прежде всего здоровья,счастья и чтобы все мечты сбылись,а так же больше медалей и репутации!С новым,2014 годом!;;;7;;;1388521406;;;4520;;;###Всех с наступающим!<br>Новогоднее обновление будет когда наберете 100 репостов <br>В обновлении будет:<br>-Зомби режим<br>-Новая тачка <br>-Режим трюков (на очки)<br>-Можно будет редактировать любые официальные карты.;;;22;;;1388476828;;;4490;;;http://cs312529.vk.me/u212511867/video/l_f01c4e27.jpg###А давайте все запомним эту дату! 31.12.2013 - в этот день, в нашу любимую игру теперь играет 100.000 замечательных гонщиков и гонщиц ! Радуемся вместе : И ещё раз всех-все-всех с НАСТУПАЮЩИМ НОВЫМ ГОДОМ!;;;2;;;1388461084;;;4483;;;###Дорогие участники группы!Администрация вас убедительно просит писать сообщения в нужные темы.Не надо писать в тему Вопросы по игре\" ваши ошибки игры.А так же просим не писать вопросы под скриншотами.Для ваших вопросов существуют соответствующие темы.Администрации просто неудобно отвечать на ваши сообщения;;;0;;;1388417150;;;4475;;;###Обновление<br>• Исправлен баг с новыми моделями,теперь сквозь них не проваливаешься.<br>• Добавлена строка,показывающая какие модели были взяты ранее,что довольно удобно при работе с малым кол-вом моделей.;;;1;;;1388389462;;;4453;;;http://cs605224.vk.me/v605224694/51e/a8ZdafYh5lg.jpg###◘ Хотите узнавать новости своей любимой игры первыми?<br>◘ Хотите смотреть обзоры лучших карт?<br>◘ Хотите участвовать в конкурсах на ценные призы?<br><br>Да?<br>_____________________<br>Тогда наше сообщество для вас :<br>http://vk.com/news_trackracing;;;0;;;1388388796;;;4452;;;http://cs413831.vk.me/v413831694/9f47/Dk_xLPLCRuQ.jpg###Вступаем в первый по созданию клан в игре —&gt; http://vk.com/racer_online_fresh;;;0;;;1388381242;;;4451;;;http://cs413829.vk.me/v413829608/4f8d/eq9F6fwENC4.jpg###Пользуйтесь :;;;1;;;1388321197;;;4425;;;http://cs605223.vk.me/v605223694/745/p-K6D5yozy0.jpg###Уважаемые игроки, появилась тема: Жалобы на игроков\". Там вы можете оставить жалобу на игроков (Скрины обязательно)   —-&gt; http://vk.com/topic-59755500_29138173;;;1;;;1388225402;;;4404;;;###;;;2;;;1388165797;;;4386;;;http://cs413829.vk.me/v413829928/5b83/SMD90a8bpyM.jpg###Придумал режим для игры, по трассе разбросаны монеты(очки),  кто их больше всех соберет за 5 минут тот выиграл!;;;7;;;1388161165;;;4379;;;###Конкурс будет окончен через 30 мин (Причину могу не называть);;;0;;;1388149305;;;4366;;;###Друзья! Гонщики! Вся администрация игры поздравляет вас с наступающим 2014 годом! Знаком года будет лошадь,которая такая-же быстрая как и мы :<br>_________<br>Скоро в честь нового года в игре будет добавлена машина,да не простая а с новогодней тематикой!<br><br>Жмите лайки если хотите машину!;;;13;;;1388068748;;;4341;;;http://cs605222.vk.me/v605222694/134/GqZyPTcugzg.jpg###Наверное многие ждали этого :<br><br>Скоро (возможно сегодня) в игре можно будет строить карты как стандартные.;;;0;;;1388018892;;;4310;;;http://cs605221.vk.me/v605221694/a9/odclQqQzvIo.jpg###");
        //}
        ExternalCall("VkInfo");
        ExternalCall("VkNews");
    }
    public List<Posts> posts = new List<Posts>();
    public void VkNews(string s)
    {
        //print("vknews " + s);
        posts.Clear();
        var strings = s.Replace("<br>", "").Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var a in strings)
        {
            try
            {
                var ss = a.Split(new[] { ";;;" }, StringSplitOptions.None);
                DateTime time = new DateTime(1970,1,1,0,0,0,0);
                time=time.AddSeconds(Int64.Parse(ss[2]));

                var item = new Posts() { msg = ss[0].Trim(), comments = Int32.Parse(ss[1]), date = time,id=ss[3], imageUrl = ss[4] };
                StartCoroutine(item.Load());
                posts.Add(item);
            }
            catch (System.Exception e) { Debug.LogError(e); }
        }
        Debug.Log("vknews " + strings.Length + ":" + posts.Count);
    }

    public void VkFriends(string fr)
    {
        print("VkFriends " + fr);
        string[] strings = fr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (_Loader.friendCount != strings.Length)
        {
            var added = _Loader.friendCount == -1 ? 0 : strings.Length - _Loader.friendCount;
            _Loader.friendCount = Mathf.Max(strings.Length, _Loader.friendCount);            
            print("<<<<<<<<<< friend added " + added);
            if (added > 0) 
            {
                int rep = added * 10;
                _Loader.reputation += rep;
                Popup2(String.Format(Tr("+{0} reputation points, you added {1} friends"), rep, added), _Loader.MenuWindow, null, false, 500, 250, res.sendReplayIcon);
            }
        }
#if old
        foreach (var a in strings)
        {
            Download(mainSite + "/dict/vk" + a.Trim() + ".txt", delegate(string s, bool b)
            {
                if (b)
                {
                    var ss = s.Split(':');
                    string trim = ss[0].Trim().ToLower();
                    AddFriend(trim);
                    dict[trim] = ss[1];
                }
            }, false);
        }
#endif
        _Loader.friends = _Loader.friends;
    }
    private static void AddFriend(string trim)
    {
        trim = trim.ToLower();
        if (!_Loader.friends.Contains(trim))
        {
            print("friend added " + trim);
            _Loader.friends.Add(trim);
        }
    }
    
    public void OnPostedWall()
    {
        if (Loader.totalSeconds - PlayerPrefs.GetInt("posted") > 60 * 3)
        {
            PlayerPrefs.SetInt("posted", Loader.totalSeconds);
            AddReputation(3);
        }
    }
    public void AddReputation(int v, string s = "+{0} reputation points")
    {
        if (v <= 0) return;
        _Loader.reputation += v;
        Popup(String.Format(Tr(s), v), res.sendReplayIcon);
    }
    public void AddMedals(int v, string s = "+{0} medals")
    {
        if (v <= 0) return;
        _Loader.medals += v;
        Popup(String.Format(Tr(s), v), res.sendReplayIcon);
    }
    public void VkInfo2(string s)
    {
        vkLoggedIn = true;        
        site = Site.VK;
        curDict = 1;
        print("Info " + s);
        var ss = s.Split(',');
        userId = ss[0];
        userName = ss[1];
        _Loader.avatarUrl = ss[2];
        _Loader.vkPassword = userId;
        Download(mainSite + "scripts/dict.php", delegate { }, true, "key", "vk" + userId, "value", _Loader.playerNamePrefixed + ":" + userName);
    }

    
    
}