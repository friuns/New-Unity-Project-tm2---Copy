<?php
/**
 * Created by JetBrains PhpStorm.
 * User: Administrator
 * Date: 24/10/13
 * Time: 11:26 PM
 * To change this template use File | Settings | File Templates.
 */
include("include.php");
set_time_limit(360);
echo "start\n";
$a = "from tmreplays2 as t1 where (select count(*) from tmreplays where map=t1.map)>10 and NOW() - INTERVAL 7 day>date";
$r = $mysqli->query("select name $a");
echo "query\n";
while($obj = $r->fetch_assoc()){
    if(file_exists2($obj['name']) && unlink($obj['name']))
        echo '1';
}
echo "delete\n";
$r = $mysqli->query("delete $a");
//$ar = explode("\r\n","delete from tmreplays WHERE  NOW() - INTERVAL 7 day>date");
//foreach($ar as $a)
//    if(!$mysqli->query($a))
//        echo(mysqli_error($mysqli));
$date= date("Y-m-d H:i:s");
$elapsed = microtime(true)-$timeStart;
file_put_contents2("clearLog.txt","\n$date Cleanup in $elapsed\n",FILE_APPEND);
echo $elapsed;