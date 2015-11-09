using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using Ionic.Zlib;
#if !UNITY_WP8
using CodeStage.AntiCheat;
using CodeStage.AntiCheat.ObscuredTypes;
#endif
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



public partial class Loader
{

#if UNITY_EDITOR


    public override void OnEditorGui()
    {
        if (gui.Button("Test"))
        {
            StringBuilder sb = new StringBuilder();
            //foreach (var a in playerPrefKeys)
            //    sb.Append(a).Append(",");
            sb.Append("Enc:Enc,country:fi,-2022168951:True,soulkeywebmodtype:100,enc:Enc,dict:0,playername:soulkey,-2022147845:True,soulkeyreputation:709,-2022168905:True,soulkeymedals:482,webquality:3,webdrawdistance:10000,webshadows:True,-2022147930:True,-1266171119:True,friunsreputation:50,ffggavatar:1,ffggwp8quality:1,ffggwp8drawdistance:200,ffggwp8shadows:False,ffggwp8playerscount:1,a03;ffgg;1:0:9.275256,a03;ffgg;1:1:17.96432,6a03;ffgg;record:17.96432,zxc3:True,ffggmedals:3,a03;ffgg;medal:1,dsa30:True,dsa20:True,friunsscore:0,dsa33:True,friendcount:12,guest5817avatar:6,guest5817androiddrawdistance:500,guest5817androidshadows:True,guest5817car:0,guest5817androidplayerscount:3,a02;guest5817;1:0:8.625241,a02;guest5817;1:1:15.77968,6a02;guest5817;record:15.77968,guest5817medals:6,a02;guest5817;medal:1,teo76.teo7;guest5817;1:0:7.040205,teo76.teo7;guest5817;1:1:19.28909,teo76.teo7;guest5817;1:2:42.8558,teo76.teo7;guest5817;1:3:55.77856,teo76.teo7;guest5817;1:4:64.1172,guest5817androidquality:3,teo76.teo7;guest5817;1:5:71.9529,teo76.teo7;guest5817;1:6:82.27222,teo76.teo7;guest5817;1:7:99.77261,teo76.teo7;guest5817;2:0:103.4006,teo76.teo7;guest5817;2:1:114.0048,teo76.teo7;guest5817;2:2:136.8168,guest5817androidshowyourghost:False,ffffffsdf.test;guest5817;1:0:18.52422,ffffffsdf.test;guest5817;1:1:28.99784,ffffffsdf.test;guest5817;1:2:40.32026,6ffffffsdf.test;guest5817;record:40.32026,ffffffsdf.test;guest5817;medal:1,teo76.teo8;guest5817;1:0:14.87984,teo76.teo8;guest5817;1:1:22.8235,teo76.teo8;guest5817;1:2:32.40857,teo76.teo8;guest5817;1:3:49.24217,teo76.teo8;guest5817;1:4:63.89732,friunsflashautoquality:false,friunsflashcontrols2:1,friunsflashquality:0,friunsflashplayerscount:3,friunsflashshadows:true,friunsstatssaved2:True,friunsavatar:2,friunsandroiddrawdistance:800,friunsandroidshadows:False,friunscar:2,friunsandroiddifficulty:0,friunsandroidplayerscount:1,lilblo.2laps;friuns;1:0:12.30027,lilblo.2laps;friuns;1:1:24.80817,lilblo.2laps;friuns;1:2:31.77344,friunsandroidcontrols2:1,audiovolume2:0.5417827,lilblo.2laps;friuns;1:3:39.59011,lilblo.2laps;friuns;1:4:47.91689,lilblo.2laps;friuns;1:5:51.37762,lilblo.2laps;friuns;1:6:56.58374,lilblo.2laps;friuns;1:7:65.72632,lilblo.2laps;friuns;1:8:69.14944,lilblo.2laps;friuns;1:9:79.2189,lilblo.2laps;friuns;1:10:85.13065,lilblo.2laps;friuns;1:11:85.61039,lilblo.2laps;friuns;1:12:93.62099,lilblo.2laps;friuns;1:13:102.9659,lilblo.2laps;friuns;1:14:113.0753,lilblo.2laps;friuns;1:15:117.328,lilblo.2laps;friuns;1:16:122.0154,lilblo.2laps;friuns;1:17:129.8049,lilblo.2laps;friuns;1:18:132.938,lilblo.2laps;friuns;1:19:135.2903,lilblo.2laps;friuns;1:20:139.9448,lilblo.2laps;friuns;1:21:146.9467,lilblo.2laps;friuns;2:0:156.7112,friunsandroidsensivity:0.5299442,lilblo.2laps;friuns;2:1:168.5427,lilblo.2laps;friuns;2:2:175.7147,lilblo.2laps;friuns;2:3:183.5624,lilblo.2laps;friuns;2:4:192.2659,lilblo.2laps;friuns;2:5:195.6492,lilblo.2laps;friuns;2:6:199.7832,lilblo.2laps;friuns;2:7:207.9912,lilblo.2laps;friuns;2:8:211.4896,lilblo.2laps;friuns;2:9:221.9999,lilblo.2laps;friuns;2:10:228.3461,lilblo.2laps;friuns;2:11:229.0067,lilblo.2laps;friuns;2:12:238.8964,lilblo.2laps;friuns;2:13:247.8451,lilblo.2laps;friuns;2:14:259.1161,lilblo.2laps;friuns;2:15:264.5564,lilblo.2laps;friuns;2:16:270.2319,lilblo.2laps;friuns;2:17:278.7452,lilblo.2laps;friuns;2:18:282.0234,lilblo.2laps;friuns;2:19:284.656,lilblo.2laps;friuns;2:20:290.3666,lilblo.2laps;friuns;2:21:297.7538,6lilblo.2laps;friuns;record:297.7538,friunsmedals:32,lilblo.2laps;friuns;medal:3,votedlilblo.2laps:True,friunswebdifficulty:1,friunswebplayerscount:6,friunswebquality:3,friunswebdrawdistance:10000,friunswebshadows:True,-reataparada.ahorafaltaqchoques5;friuns;1:0:11.3703,-reataparada.ahorafaltaqchoques5;friuns;1:1:20.92382,-reataparada.ahorafaltaqchoques5;friuns;1:2:27.17277,-reataparada.ahorafaltaqchoques5;friuns;1:3:31.42336,-reataparada.ahorafaltaqchoques5;friuns;1:4:45.37634,sparda.sparda;friuns;1:0:4.850155,sparda.sparda;friuns;1:1:6.470192,sparda.sparda;friuns;1:2:8.540239,sparda.sparda;friuns;1:3:13.55506,sparda.sparda;friuns;1:4:19.16912,sparda.sparda;friuns;1:5:20.08396,sparda.sparda;friuns;1:6:23.50339,sparda.sparda;friuns;1:7:27.55771,sparda.sparda;friuns;1:8:32.64862,sparda.sparda;friuns;1:9:37.2246,6sparda.sparda;friuns;record:37.2246,sparda.sparda;friuns;medal:1,votedsparda.sparda:True,d07;friuns;1:0:12.77519,d07;friuns;1:1:25.04813,d07;friuns;1:2:34.21396,d07;friuns;1:3:41.97562,d07;friuns;1:4:51.19759,autofullscreen:False,a10;friuns;1:0:7.415214,6a10;friuns;record:7.415214,a10;friuns;medal:3,friunswp8controls2:1,friunswp8quality:1,friunswp8playerscount:0,friunswp8drawdistance:261,a03;friuns;1:0:8.950249,a03;friuns;1:1:17.61438,6a03;friuns;record:17.61438,zxc6:True,a03;friuns;medal:3,a02;friuns;1:0:8.389327,a02;friuns;1:1:14.94555,6a02;friuns;record:14.94555,-1266180513:True,a02;friuns;medal:3,c1;friuns;1:0:7.16122,c1;friuns;1:1:17.68059,c1;friuns;1:2:31.2468,6c1;friuns;record:31.2468,-1266180528:True,c1;friuns;medal:3,e01;friuns;1:0:27.40412,e01;friuns;1:1:34.61642,e01;friuns;1:2:47.20126,6e01;friuns;record:47.20126,-1266180526:True,e01;friuns;medal:2,playedtimes:170,msgid:162,d4;friuns;1:0:10.29731,d4;friuns;1:1:17.92313,friunswebcontrols2:1,friunswebmusicvolume2:0,friunswebautoquality:True,d12;friuns;1:0:5.897191,d12;friuns;1:1:20.43377,d12;friuns;1:2:29.12312,d12;friuns;1:3:35.96751,d12;friuns;1:4:41.06767,6d12;friuns;record:41.06767,-1266180525:True,d12;friuns;medal:2,-1266180524:True,-1266180530:True,-1266171097:True,-1266180488:True,-1266171075:True,d9;friuns;1:0:21.62364,-599401285:True,b04;friuns2;0:13.98998,6b04;friuns2;record:13.98998,friuns2medals:9,b04;friuns2;medal:3,c06;friuns2;0:12.24528,c06;friuns2;1:38.34484,c06;friuns2;2:50.26239,c06;friuns2;3:62.3032,6c06;friuns2;record:62.3032,c06;friuns2;medal:1,friuns2statssaved2:True,friuns2webquality:3,friuns2webdrawdistance:10000,friuns2webshadows:True,friuns2dddwebquality:3,friuns2dddwebdrawdistance:10000,friuns2dddwebshadows:True,friuns2ddddaybonus:1,friuns2dddlastdayplayed:735185,friuns2dddddaybonus:1,friuns2ddddlastdayplayed:735185,friuns2ddddwebquality:3,friuns2ddddwebdrawdistance:10000,friuns2ddddwebshadows:True,friuns2dddddwebquality:3,friuns2dddddwebdrawdistance:10000,friuns2dddddwebshadows:True,friuns2dddddavatar:2,friuns2ddddddaybonus:0,friuns2dddddlastdayplayed:735185,355738504:True,friuns2dddddmedals:2,355645121:True,friuns2ddavatar:2,friuns2dddaybonus:2,friuns2ddlastdayplayed:735185,-706513742:True,friuns2ddmedals:18,-706354693:True,friuns2ddwebquality:3,friuns2ddwebdrawdistance:10000,friuns2ddwebshadows:True,-706513738:True,-706513736:True,-706513732:True,-706513758:True,friuns2ddwebdifficulty:1,-2022147928:True,-2022148041:True,uuuuufriends:-uuuuu,uuuuuavatar:2,uuuuuwebquality:3,uuuuuwebdrawdistance:10000,uuuuuwebshadows:True,inkvizitor.syper;uuuuu;1:0:1.395076,inkvizitor.syper;uuuuu;1:1:6.090183,inkvizitor.syper;uuuuu;1:2:11.0503,inkvizitor.syper;uuuuu;1:3:14.56989,inkvizitor.syper;uuuuu;1:4:21.97864,inkvizitor.syper;uuuuu;1:5:26.89782,uuuuuwebdifficulty:1,nikita113.karta1223;uuuuu;1:0:5.06016,nikita113.karta1223;uuuuu;1:1:21.41874,-artem.dolga;uuuuu;1:0:3.910133,-artem.dolga;uuuuu;1:1:7.650219,-artem.dolga;uuuuu;1:2:9.395259,-artem.dolga;uuuuu;1:3:11.2503,-artem.dolga;uuuuu;1:4:12.85018,-2022147094:True,qweavatar:1,qwewebdifficulty:1,qwewebquality:3,qwewebdrawdistance:10000,-2022147366:True,soulkeyfriends:Soulkey,soulkeywebdifficulty:1,soulkeywebdrawdistance:10000,soulkeywebshadows:True,soulkeywebcontrols2:1,soulkeywebautoquality:False,soulkeystatssaved2:True,soulkeyavatar:1,-2022168733:True,-2022147376:True,-2022168707:True,-2022147386:True,-2022168713:True,-2022147268:True,-2022168767:True,-2022147278:True,-2022168741:True,-2022147288:True,-2022168747:True,-2022147298:True,-2022168785:True,-2022147308:True,-2022168775:True,-2022147318:True,-2022168781:True,-2022147328:True,-2022168819:True,-2022147210:True,-2022168825:True,-2022147220:True,-2022168815:True,-2022147230:True,-2022168597:True,-2022147240:True,-2022168603:True,-2022147250:True,-2022168577:True,-2022147260:True,-2022168631:True,-2022147142:True,-2022168637:True,-2022147152:True,-2022168611:True,-2022147162:True,-2022168617:True,-2022147172:True,-2022168671:True,-2022147182:True,-2022168645:True,-2022147192:True,-2022168651:True,-2022147074:True,-2022168689:True,-2022147084:True,-2022168679:True,soulkeywebquality:3,soulkey2drcouperepunlocked:True,soulkey2drcoupepaintunlocked:True,soulkey2drcouper:0.2509804,soulkey2drcoupeg:0.9411765,soulkey2drcoupeb:0.2313726,soulkey2drcoupecolor:True,soulkeycar:0,clantag:,-2024353976:True,-2022148040:True,soulkeywebplayerscount:6,a02;soulkey;1:0:7.918144,a02;soulkey;1:1:14.98544,6a02;soulkey;record:14.98544,-2022168677:True,a02;soulkey;medal:1,a03;soulkey;1:0:7.972847,a03;soulkey;1:1:16.5516,6a03;soulkey;record:16.5516,-2022168676:True,a03;soulkey;medal:1,-2022147924:True,ddddddddfriends:-dddddddd,ddddddddavatar:2,ddddddddwebdifficulty:1,ddddddddwebquality:3,ddddddddwebdrawdistance:10000,ddddddddwebshadows:True,ddddddddwebrearcamera:True,dimonvision.russiaandukraina2;dddddddd;1:0:2.218517,dimonvision.russiaandukraina2;dddddddd;1:1:5.721514,dimonvision.russiaandukraina2;dddddddd;1:2:7.422992,dimonvision.russiaandukraina2;dddddddd;1:3:11.57969,dimonvision.russiaandukraina2;dddddddd;1:4:12.71472,dimonvision.russiaandukraina2;dddddddd;1:5:14.83685,dimonvision.russiaandukraina2;dddddddd;1:6:29.21646,dimonvision.russiaandukraina2;dddddddd;1:7:31.01854,dimonvision.russiaandukraina2;dddddddd;1:8:33.55064,dimonvision.russiaandukraina2;dddddddd;1:9:37.05173,ddddddddwebplayerscount:6,inarocka2002.skrileks;dddddddd;1:0:6.055134,d15;dddddddd;1:0:2.869108,d15;dddddddd;1:1:8.039318,d15;dddddddd;1:2:15.14972,d15;dddddddd;1:3:23.78593,dimonvision.russiaandukraina2;dddddddd;1:10:94.92413,dimonvision.russiaandukraina2;dddddddd;1:11:95.93907,dimonvision.russiaandukraina2;dddddddd;2:0:98.80922,dimonvision.russiaandukraina2;dddddddd;2:1:103.0807,dimonvision.russiaandukraina2;dddddddd;2:2:105.0855,dimonvision.russiaandukraina2;dddddddd;2:3:109.6466,dimonvision.russiaandukraina2;dddddddd;2:4:110.8146,dimonvision.russiaandukraina2;dddddddd;2:5:112.8197,dimonvision.russiaandukraina2;dddddddd;2:6:154.9194,dimonvision.russiaandukraina2;dddddddd;2:7:157.8873,a02;;1:0:8.730244,a02;;1:1:16.2996,6a02;;record:16.2996,medals:2,a02;;medal:2,yesavatar:2,yeswebdifficulty:1,yeswebquality:3,yeswebdrawdistance:10000,yeswebshadows:True,yeswebplayerscount:3,-jet.minotaur;yes;1:0:5.50017,-jet.minotaur;yes;1:1:9.225255,-jet.minotaur;yes;1:2:13.66004,-jet.minotaur;yes;1:3:19.44407,-jet.minotaur;yes;1:4:32.34356,-jet.minotaur;yes;1:5:35.68927,-jet.minotaur;yes;1:6:40.77536,-jet.minotaur;yes;1:7:46.93167,-jet.minotaur;yes;1:8:56.54873,6-jet.minotaur;yes;record:56.54873,yesmedals:7,-jet.minotaur;yes;medal:2,a02;yes;1:0:8.215232,a02;yes;1:1:14.86984,6a02;yes;record:14.86984,a02;yes;medal:1,wp8quality:3,wp8drawdistance:500,wp8shadows:True,frwp8quality:1,frwp8drawdistance:200,frwp8shadows:False,soulkeywp8playerscount:3,soulkeywp8quality:0,soulkeywp8drawdistance:150,soulkeywp8shadows:False,a06;soulkey;1:0:6.790199,a06;soulkey;1:1:21.28876,a06;soulkey;1:2:24.41823,6a06;soulkey;record:29.25818,a06;soulkey;medal:1,soulkeywp8controls2:1,soulkeywp8autoquality:False,c03;soulkey;1:0:10.14807,6c03;soulkey;record:10.14807,c03;soulkey;medal:1,b7;soulkey;1:0:5.52017,b7;soulkey;1:1:15.52973,b7;soulkey;1:2:18.26427,b7;soulkey;1:3:23.16344,b7;soulkey;1:4:26.13295,6b7;soulkey;record:26.13295,b7;soulkey;medal:1,mstthslava.fresh;soulkey;1:0:27.66769,votedmstthslava.fresh:True,mstthslava.fresh;soulkey;1:1:35.7893,mstthslava.fresh;soulkey;1:2:41.17545,mstthslava.fresh;soulkey;1:3:49.4072,mstthslava.fresh;soulkey;1:4:57.24888,mstthslava.fresh;soulkey;1:5:61.53862,mstthslava.fresh;soulkey;1:6:65.08667,mstthslava.fresh;soulkey;1:7:72.14779,mstthslava.fresh;soulkey;1:8:78.45432,mstthslava.fresh;soulkey;1:9:84.64592,mstthslava.fresh;soulkey;1:10:90.75756,mstthslava.fresh;soulkey;1:11:93.57601,mstthslava.fresh;soulkey;1:12:103.4006,mstthslava.fresh;soulkey;1:13:105.6094,6mstthslava.fresh;soulkey;record:105.6094,mstthslava.fresh;soulkey;medal:1,mstthslava.fresh1;soulkey;1:0:6.500193,mstthslava.fresh1;soulkey;1:1:13.21012,mstthslava.fresh1;soulkey;1:2:18.88916,mstthslava.fresh1;soulkey;1:3:32.44358,mstthslava.fresh1;soulkey;1:4:40.73535,mstthslava.fresh1;soulkey;1:5:50.63747,mstthslava.fresh1;soulkey;1:6:57.819,mstthslava.fresh1;soulkey;1:7:66.8457,mstthslava.fresh1;soulkey;1:8:73.7619,mstthslava.fresh1;soulkey;1:9:75.29106,mstthslava.fresh1;soulkey;1:10:78.35938,mstthslava.fresh2;soulkey;1:0:8.280233,mstthslava.fresh2;soulkey;1:1:11.75031,mstthslava.fresh2;soulkey;1:2:16.69953,mstthslava.fresh2;soulkey;1:3:20.78884,mstthslava.fresh2;soulkey;1:4:23.64336,mstthslava.fresh2;soulkey;1:5:27.77267,egor65445368.buduee;soulkey;1:0:3.435122,egor65445368.buduee;soulkey;1:1:7.170208,egor65445368.buduee;soulkey;1:2:8.935248,egor65445368.buduee;soulkey;1:3:12.55023,egor65445368.buduee;soulkey;1:4:16.3096,egor65445368.buduee;soulkey;1:5:33.22375,egor65445368.buduee;soulkey;1:6:51.75771,egor65445368.buduee;soulkey;1:7:58.81921,egor65445368.buduee;soulkey;1:8:62.86789,egor65445368.buduee;soulkey;1:9:73.57201,egor65445368.buduee;soulkey;1:10:79.74861,egor65445368.buduee;soulkey;1:11:82.36717,egor65445368.buduee;soulkey;1:12:88.05405,egor65445368.buduee;soulkey;1:13:94.44553,egor65445368.buduee;soulkey;1:14:106.339,egor65445368.buduee;soulkey;1:15:121.5157,6egor65445368.buduee;soulkey;record:121.5157,egor65445368.buduee;soulkey;medal:1,votedegor65445368.buduee:True,a10;soulkey;1:0:6.465192,6a10;soulkey;record:6.465192,a10;soulkey;medal:2,qamar.trace;soulkey;1:0:0.03504467,qamar.trace;soulkey;1:1:2.505101,qamar.trace;soulkey;1:2:5.405168,qamar.trace;soulkey;1:3:9.830269,votedqamar.trace:True,qamar.trace;soulkey;1:4:19.854,qamar.trace;soulkey;1:5:28.88282,qamar.trace;soulkey;1:6:33.37878,qamar.trace;soulkey;1:7:34.72907,qamar.trace;soulkey;1:8:36.15437,qamar.trace;soulkey;1:9:43.81101,qamar.trace;soulkey;1:10:49.44721,6qamar.trace;soulkey;record:49.44721,qamar.trace;soulkey;medal:3,wolkodav463.vametoneproiti;soulkey;1:0:0.03004456,wolkodav463.vametoneproiti;soulkey;1:1:13.71503,wolkodav463.vametoneproiti;soulkey;1:2:20.05397,wolkodav463.vametoneproiti;soulkey;1:3:22.48356,wolkodav463.vametoneproiti;soulkey;1:4:38.38485,wolkodav463.vametoneproiti;soulkey;1:5:42.95082,wolkodav463.vametoneproiti;soulkey;1:6:51.37262,wolkodav463.vametoneproiti;soulkey;1:7:61.13384,wolkodav463.vametoneproiti;soulkey;1:8:72.1228,wolkodav463.vametoneproiti;soulkey;1:9:81.37772,wolkodav463.vametoneproiti;soulkey;1:10:84.00627,wolkodav463.vametoneproiti;soulkey;1:11:95.415,wolkodav463.vametoneproiti;soulkey;1:12:100.2973,wolkodav463.vametoneproiti;soulkey;1:13:104.9847,wolkodav463.vametoneproiti;soulkey;1:14:108.6177,wolkodav463.vametoneproiti;soulkey;1:15:122.69,6wolkodav463.vametoneproiti;soulkey;record:122.69,wolkodav463.vametoneproiti;soulkey;medal:2,-guest2114.bd020;soulkey;1:0:37.72471,voted-guest2114.bd020:True,-dimovsky.loops;soulkey;1:0:19.02914,-dimovsky.loops;soulkey;1:1:32.09851,soulkeywebenablebloom:True,-dimovsky.loops;soulkey;1:2:49.8773,-dimovsky.loops;soulkey;1:3:62.04834,-dimovsky.loops;soulkey;1:4:72.28771,-dimovsky.loops;soulkey;1:5:85.40051,6-dimovsky.loops;soulkey;record:85.40051,-dimovsky.loops;soulkey;medal:1,-esteban.cabj;soulkey;1:0:2.415099,-esteban.cabj;soulkey;1:1:5.130161,inkvizitor.syper;soulkey;1:0:1.300074,inkvizitor.syper;soulkey;1:1:5.860178,inkvizitor.syper;soulkey;1:2:10.68529,inkvizitor.syper;soulkey;1:3:14.06497,inkvizitor.syper;soulkey;1:4:21.73868,inkvizitor.syper;soulkey;1:5:27.19777,inkvizitor.syper;soulkey;1:6:34.08893,inkvizitor.syper;soulkey;1:7:41.27547,inkvizitor.syper;soulkey;1:8:42.93082,inkvizitor.syper;soulkey;1:9:48.16694,inkvizitor.syper;soulkey;1:10:49.02212,c1;soulkey;1:0:7.355212,c1;soulkey;1:1:18.60224,c1;soulkey;1:2:31.54839,6c1;soulkey;record:40.84716,c1;soulkey;medal:3,mstthslava.fresh1v2;soulkey;1:0:2.305097,mstthslava.fresh1v2;soulkey;1:1:9.640265,mstthslava.fresh1v2;soulkey;1:2:12.27527,mstthslava.fresh1v2;soulkey;1:3:18.35925,mstthslava.fresh1v2;soulkey;1:4:21.87866,mstthslava.fresh1v2;soulkey;1:5:25.76301,mstthslava.fresh1v2;soulkey;1:6:28.01764,mstthslava.fresh1v2;soul");
            print(sb.Length);
            var ss = GZipStream.CompressString(sb.ToString());
            print(Convert.ToBase64String(ss).Length);
            print(ss.Length);
        }

        playerName = gui.TextField(playerName);
        //medals = EditorGUILayout.IntField("medals", medals);
        //reputation = EditorGUILayout.IntField("reputation", reputation);
        //carSkin = EditorGUILayout.IntField("Car Skin", carSkin);
        if (GUILayout.Button("Reset Settings"))
            PlayerPrefsClear();
#if old
        if (GUILayout.Button("Merge Dicts"))
        {
            var splitString = SplitString(assetDictionaryMerge.text);
            var dest = SplitString(assetDictionaryMergeTo.text);
            bool merge = false;
            for (int i = 0, j = 0; i < dest.Length; i++)
            {

                dest[i] = dest[i].Trim(';');
                if (dest[i] == "_merge_")
                    merge = true;
                else if (merge)
                {
                    print("merged " + splitString[j]);
                    dest[i] += ";" + splitString[j].Trim(';') + ";";
                    j++;
                }
            }
            WriteAllLines(UnityEditor.AssetDatabase.GetAssetPath(_Loader.assetDictionaryMergeTo), dest);
        }
#endif
        medals = EditorGUILayout.IntField("medals", medals);
        reputation = EditorGUILayout.IntField("reputation", reputation);
        //OnEditorGuiOld();
        base.OnEditorGui();
    }

    //protected IEnumerator TakeScreenshot()
    //{
    //    yield return new WaitForEndOfFrame();
    //    int width = Screen.width;
    //    int height = Screen.height;
    //    Texture2D tx = new Texture2D(width, height, TextureFormat.RGB24, false);
    //    tx.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    //    tx.Apply();
    //    yield return new WaitForEndOfFrame();
    //    byte[] screenshotBytes = tx.EncodeToPNG();
    //    Destroy(tx);
    //    WWWForm form = new WWWForm();
    //    form.AddField("name", playerName);
    //    form.AddBinaryData("file", screenshotBytes);

    //    var w = new WWW(mainsite + "uploadScreenshot.php", form);
    //    _CustomWindow.ShowWindow(delegate { gui.Label("Uploading Screenshot"); });
    //    yield return w;
    //    _CustomWindow.ShowWindow(delegate { gui.Label("Copy screenshot url to clipboard"); gui.TextArea(w.text); });
    //}
#if old

    public void OnEditorGuiOld()
    {
        if (gui.Button("LoadScenes"))
            LoadScenes();
        if (gui.Button("InitCars"))
        {
            for (int i = 0; i < res.CarSkins.Count; i++)
            {
                int a = Mathf.RoundToInt((float)i / (res.CarSkins.Count - 1) * 4f);
                CarSkin c = res.CarSkins[i];
                c.Speed = a;
                c.Rotation = 4 - a;
                print(c.prefabName);
                print("speed:" + c.getSpeed());
                print("rotation:" + c.getRotation());
            }
            SetDirty(res);
        }

        if (levelEditor != null)
            _Loader.AproveMap();
        foreach (var a in resEditor.renderSettings)
            if (GUILayout.Button(a.name))
                a.Active();

        playerName = gui.TextField(playerName);

        carSkin = EditorGUILayout.IntField("Car Skin", carSkin);
        if (GUILayout.Button("Reset Settings"))
            PlayerPrefsClear();
        if (GUILayout.Button("Merge Dicts"))
        {
            var splitString = SplitString(assetDictionaryMerge.text);
            var dest = SplitString(assetDictionaryMergeTo.text);
            bool merge = false;
            for (int i = 0, j = 0; i < dest.Length; i++)
            {

                dest[i] = dest[i].Trim(';');
                if (dest[i] == "_merge_")
                    merge = true;
                else if (merge)
                {
                    print("merged " + splitString[j]);
                    dest[i] += ";" + splitString[j].Trim(';') + ";";
                    j++;
                }
            }
            WriteAllLines(UnityEditor.AssetDatabase.GetAssetPath(_Loader.assetDictionaryMergeTo), dest);
        }
    }
#endif
    //private void SortScenes()
    //{
    //    scenes.Sort(scenes[0]);
    //    foreach (var a in scenes)
    //        a.name = a.texture.name.ToLower() + (a.Enabled ? "" : " (disabled)");
    //    SetDirty();
    //}
#endif   
}