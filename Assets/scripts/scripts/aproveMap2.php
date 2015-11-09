<?php
include('include.php');
$map = get('map');
$tested = get('tested');
$user = tryget('aprovedBy');
$advanced = tryget('advanced',5000,0);
query("update tmusermaps set tested='$tested', aprovedBy='$user',date = Now(),advanced='$advanced'  where name='$map'") or trigger_error('');