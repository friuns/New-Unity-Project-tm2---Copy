<?php
include('inc/include.php');
rrmdir('msgcheck');
rrmdir('logs');
rrmdir('gamelogs');

function rrmdir($dir) {
	foreach(glob($dir . '/*') as $file) {
		if(is_dir($file))
			rrmdir($file);
		else
			unlink($file);
	}
	//rmdir($dir);
}

$ar = explode("\r\n","
update maps set survivalPlays=0, singlePlays=0, classicPlays=0, tdmPlays=0;
update users set votesLeft=3;
update users set KillsToday=0, DeathsToday=0;
delete from featurereq where NOW() - INTERVAL 7 day>date and aproved=0;
TRUNCATE TABLE `errors`;
TRUNCATE TABLE `errors2`;
TRUNCATE TABLE `perf`;
TRUNCATE TABLE `compare`;
TRUNCATE TABLE allowonce;
insert into userCountResult select CURDATE() as dt,device,count(*) from usercount2 GROUP BY device;
TRUNCATE usercount2;
delete from users where NOW() - INTERVAL 30 day>lastLogin and Kills<100;

");
foreach($ar as $a)
	mysql_query($a) or die(mysql_error());
