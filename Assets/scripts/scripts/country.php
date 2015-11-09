<?php
include("geoip.inc");

$gi = geoip_open("GeoIP.dat",GEOIP_STANDARD);
$country = geoip_country_code_by_addr($gi, $_SERVER['REMOTE_ADDR']);
geoip_close($gi);
file_put_contents("../events2/country/$country.txt",'1',FILE_APPEND);
echo $country;
