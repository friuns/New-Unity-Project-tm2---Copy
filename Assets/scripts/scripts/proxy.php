<?php

$proxy_headers = array(
      
        'HTTP_X_FORWARDED_FOR',
        'HTTP_FORWARDED_FOR',
        'HTTP_X_FORWARDED',
        'HTTP_FORWARDED',
        'HTTP_CLIENT_IP',
        'HTTP_FORWARDED_FOR_IP',        
        'X_FORWARDED_FOR',
        'FORWARDED_FOR',
        'X_FORWARDED',
        'FORWARDED',
        'CLIENT_IP',
        'FORWARDED_FOR_IP',
        
    );
    foreach($proxy_headers as $x){
        if (isset($_SERVER[$x])) die("proxy detected $_SERVER[$x]");
    }
	
	echo "_success_"; 
	
	/*
	'HTTP_PROXY_CONNECTION'
	  'HTTP_VIA',
	  'VIA',
	*/