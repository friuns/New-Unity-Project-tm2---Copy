
<a href="javascript:history.go(-1);">&lt;&lt Go Back</a>
<?php
error_reporting(E_ALL ^ E_NOTICE);
$target_path = "usermaps/";

$allowedExtensions = array("dae","unity3dWebPlayer"); 


if ($_FILES['uploadedfile']['tmp_name']> '') 
{ 
	if (!in_array(end(explode(".", strtolower($_FILES['uploadedfile']['name']))), $allowedExtensions)) 
	{ 
		die($file['name'].' is an invalid file type!'); 
	} 
	
	$target_path = $target_path . basename( $_FILES['uploadedfile']['name']); 

	if(move_uploaded_file($_FILES['uploadedfile']['tmp_name'], $target_path)) {
		
		die( "The map ".  basename( $_FILES['uploadedfile']['name']). 
			" has been uploaded");
	} else{
		die ("There was an error uploading the file, please try again!");
	}
}

?>
<form enctype="multipart/form-data" action="uploadMap.php" method="POST">
<input type="hidden" name="MAX_FILE_SIZE" value="10485760" />
Choose a map to upload: <input name="uploadedfile" type="file" accept=".dae,unity3dWebPlayer" /><br />
<input type="submit" value="Upload File" />
</form>


