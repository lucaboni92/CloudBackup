# CloudBackup
CloudBackup is a client-server C# project for Windows OS, which allows to realize a complete backup of a custom directory in the client filesystem. It is also possible to restore a deleted file, restore previous versions of a file and restore the whole last snapshot known by server, in case of loss of the directory.

# Parameters

    Server.exe [param]

        -a   <ip address> 	  set a custom IP address for server (different from default IP read from NIC)
        -p   <port> 	      set a custom port for server (different from default 8888)
        --local 		      set localhost as IP address (127.0.0.1)
        --help 		          print help
