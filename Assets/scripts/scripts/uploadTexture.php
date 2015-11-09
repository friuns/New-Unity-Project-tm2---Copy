<?php
include('include.php');
$tmp_file_name = $_FILES['Filedata']['tmp_name'];
crdir("uploads/$_SERVER[REMOTE_ADDR]");
move_uploaded_file2($tmp_file_name, "uploads/$_SERVER[REMOTE_ADDR]/".time().".jpg");
