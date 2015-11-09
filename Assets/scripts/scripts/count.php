<?php
//include('include.php');
include('includeOnce.php');
$platform = tryget('platform');
$event  = tryget('event');
$submit = tryget('submit');
$version = tryget('version');

$date= date('Y-m-d');

if($submit)
{
    $submit=preg_replace('/[^a-zA-Z\/0-9_-]/','', $submit);
    $dir="events2/events/".substr($submit,0, strrpos($submit, '/'));
    crdir($dir);
    file_put_contents2("events2/events/$submit.txt",'1',FILE_APPEND);
}
else if($event)
{
    /*
    if(strpos($event,'/referers/')!==false)
    {
        $event = substr(strrchr($event, "/"), 1);
        //$dir="events/$date/".substr($event,0, strrpos($event, '/'));
        crdir("events/$date/");
        file_put_contents2("events/$date/$event.txt",'1',FILE_APPEND);
        die();
    }

    die($event);
*/
}
else
{
    if(isset($_SERVER['HTTP_REFERER']))
    {
        $host = parse_url($_SERVER['HTTP_REFERER'], PHP_URL_HOST);
        $host = str_ireplace('www.', '',$host);
        $broken= $platform? '':"broken/";
        $dir = "events2/referers/$date/$broken";
        crdir($dir);
        file_put_contents2("$dir$host"."_$version.txt","1",FILE_APPEND);
    }
	file_put_contents2("events2/countip/$date$platform.txt",'1',FILE_APPEND);
	//if($name)
	//	crdir("count/$date/$name");

	die("success");
}

////perf();