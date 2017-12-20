# PoolOverSocks5

PoolOverSocks5 is a simple relay application that will create a local socket to foward pool data over a socks5 proxy, similar to `torsocks` for `tor`.

---

### Usage

Please make sure you have .NET Core installed.

```sh
$ dotnet PoolOverSocks5.dll [pool address:port] [socks5 proxy address:port] [local address:port]
$ dotnet PoolOverSocks5.dll pool.usxmrpool.com:3333 127.0.0.1:9050 127.0.0.1:3333
```

![Demo working with XMR-Stak, and Tor](https://raw.githubusercontent.com/Elycin/PoolOverSocks5/master/PoolOverSocks5/images/xmr_stak_tor_demo.png)

- Your miner should connect to the local address.
- Your socks5 proxy must already be running or you will encounter a runtime exception.

---

### Current Features

  - Synchronous, only one connection at a time.
  - Command Line Argument parsing
  - Works with any currency or pool that sends data within the buffer range of 1024 bytes.

---

### How it's built

* [.NET Core] - The multiplatform framework that this is written in
* [Starksoft.Aspen] - .NET Library for handling TCP sockets over Socks5

---

### Contribution

Contribution is always welcome in the form of a issue or a pull request to improve this software.

---

### Building from source
 - clone the repository
 - install Visual Studio 2017 and .NET Core
 - load the project

---

### Todos

 - Convert relay to asynchronous, multiple clients.


 [.NET Core]: <https://www.microsoft.com/net/learn/get-started/windows>
 [Starksoft.Aspen]: <https://github.com/bentonstark/starksoft-aspen>

---

### Donations
Appreciate my work? thanks!  
If you'd like to buy me a drink you can donate using one of the addresses below:
```
BTC: 1MwzVSXVfm1Gfvtc2n3vqam8434cGA5GgT
ETH: 0xA6b57B0d2b22c6B0031CdC6c3a2953eF93d368e8
LTC: LREiRMXV3BLFAtgXF1uw5QNU2LzGcLbdM1
```
