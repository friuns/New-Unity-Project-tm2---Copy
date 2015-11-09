<?php
//
//
//
//include("inc/include.php");
//
//$random = tryget("rand");
//$key= "$random fb";
//
//require 'src/facebook.php';
//$facebook = new Facebook(array(
//	'appId'  => '164033843697118',
//	'secret' => '4e8bb0ff161db0b450bbf7c0a42b1275',
//	'cookie' => false
//	));
//
//
//$user = $facebook->getUser();
//
//if($user)
//{
//	apc_store($key,$facebook->getAccessToken(),600); 		
//	echo "success";	
//}
//else
//	header("Location:".$facebook->getLoginUrl());
//
////
////	if ($user) {
////	try {
////		//$user_profile = $facebook->api('/me');
////		echo $facebook->getAccessToken();
////		//print_r($user_profile );
////		//$status = $facebook->api('/me/feed', 'POST', array('message' => 'This post came from my app.'));
////		//var_dump($status);
////
////	} catch (FacebookApiException $e) {
////		error_log($e);
////		$user = null;
////	}
////}
////
//
