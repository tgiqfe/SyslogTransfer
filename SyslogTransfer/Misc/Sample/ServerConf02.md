# Syslogサーバ側の設定メモ 02

## Syslog出力

### ファシリティごとに対象外設定

初期状態は、どこかに記述されている、
```
*.*;auth,authpriv.none              -/var/log/syslog
```

の設定により、アプリケーションが送ったSyslogデータは全て``/var/log/syslog``にまとめられて、闇鍋になってしまう。

事前に決めたファシリティを、闇鍋対象外にする場合は、  
(ファシリティ``local0``を対象外にする場合)
```
*.*;auth,authpriv.none,local0.none              -/var/log/syslog
```

のように設定。

※ファイル名の前の「- (ハイフン)」は、非同期書き込みするという設定。  
「-」無しの場合は同期書き込み。  
非同期書き込みのほうがパフォーマンスが良いとのこと。

### 対象ファシリティを特定のファイルに設定

``rsyslog.conf``に追記もしくは、``rsyslog.d``配下に、任意のconfファイルを作成。

事前に決めたファシリティのログ出力先を設定するには、  
(ファシリティ``local0``の場合)
```
local0.debug            -/var/log/test.log
```

## 設定ファイルの構文チェック

```
rsyslogd -N 1
rsyslogd -N 1 -f /etc/rsyslog.d/10-sample.conf
```

特に構文に問題が無い場合は、
```
rsyslogd: version 8.2001.0, config validation run (level 1), master config /etc/rsyslog.d/60-test.conf
rsyslogd: End of config validation run. Bye.
```

のように出力される。


