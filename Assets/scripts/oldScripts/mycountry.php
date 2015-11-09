<?php

include("geoip.inc");

$gi = geoip_open("GeoIP.dat",GEOIP_STANDARD);
$country = geoip_country_code_by_addr($gi, $_SERVER['REMOTE_ADDR']);
geoip_close($gi);

echo $country;
