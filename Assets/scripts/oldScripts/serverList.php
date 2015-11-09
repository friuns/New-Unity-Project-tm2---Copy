<?php
include('inc/include.php');


$rs = mysql_query("select * from servers where up=1 order by count desc") or trigger_error("");

while($row = mysql_fetch_array($rs))
{
	echo "$row[name]\r\n$row[ip]\r\n";
}

