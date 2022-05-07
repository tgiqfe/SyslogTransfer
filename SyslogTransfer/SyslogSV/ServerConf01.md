# Syslogサーバ側の設定メモ 01


## UDP

設定箇所(変更前)
```conf:/etc/rsyslog.conf
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

設定箇所(変更後)
```conf:/etc/rsyslog.conf
# provides UDP syslog reception
module(load="imudp")
input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

## TCP

### 暗号化無し

設定箇所(変更前)
```conf:/etc/rsyslog.conf
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

設定箇所(変更後)
```conf:/etc/rsyslog.conf
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
module(load="imtcp")
input(type="imtcp" port="514")
```

### TLS暗号化(クライアント証明無し)

事前にインストールが必要なパッケージ
```Ubuntu
apt-get install gnutls-utils
apt-get install rsyslog-gnutls
```

```CentOS
dnf install gnutls-utils
dnf install rsyslog-gnutls
```

設定箇所(変更前)
```conf:/etc/rsyslog.conf
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

設定箇所(変更後)
```conf:/etc/rsyslog.conf
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
module(
  load="imtcp"
  StreamDriver.Name="gtls"
  StreamDriver.Mode="1"
  StreamDriver.Authmode="anon"
)
input(type="imtcp" port="514")

global(
  DefaultNetstreamDriver="gtls"
  DefaultNetstreamDriverCAFile="/etc/rsyslog.d/tls/rootCA.crt"
  DefaultNetstreamDriverCertFile="/etc/rsyslog.d/tls/cert.crt"
  DefaultNetstreamDriverKeyFile="/etc/rsyslog.d/tls/cert.key"
)
```

``StreamDriver.Authmode``の値を``anon``に設定


### TLS暗号化(クライアント証明有り)

事前にインストールが必要なパッケージ  
⇒クライアント証明無しの項を参照


設定箇所(変更前)
```conf:/etc/rsyslog.conf
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

設定箇所(変更後)
```conf:/etc/rsyslog.conf
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
module(
  load="imtcp"
  StreamDriver.Name="gtls"
  StreamDriver.Mode="1"
  StreamDriver.Authmode="x509/name"
  PermittedPeer=["クライアント証明書のCN名"]
)
input(type="imtcp" port="514")

global(
  DefaultNetstreamDriver="gtls"
  DefaultNetstreamDriverCAFile="/etc/rsyslog.d/tls/rootCA.crt"
  DefaultNetstreamDriverCertFile="/etc/rsyslog.d/tls/cert.crt"
  DefaultNetstreamDriverKeyFile="/etc/rsyslog.d/tls/cert.key"
)
```

``StreamDriver.Authmode``の値を``x509/name``に設定

``PermittedPeer``の値は、クライアント証明書のCN名に合わせる。  
CN名は、FQDNで指定すること。

暗号化通信をする場合、ルート証明書(あるいは必要な中間証明書)を、クライアント側にも予めインストールしておく必要有り。




