# PoolOverSocks5
[![Build Status](https://travis-ci.org/Elycin/PoolOverSocks5.svg?branch=master)](https://travis-ci.org/Elycin/PoolOverSocks5)

PoolOverSocks5 is a simple relay application that will create a local socket to foward pool data over a socks5 proxy, similar to `torsocks` for `tor`.

---

### Usage

Please make sure you have .NET Core installed.

```sh
$ dotnet run
```

![Demo working with XMR-Stak, and Tor](https://raw.githubusercontent.com/Elycin/PoolOverSocks5/master/PoolOverSocks5/images/xmr_stak_tor_demo.png)

- Your miner should connect to the local address.
- Your socks5 proxy must already be running or you will encounter a runtime exception.

---

### Current Features
  - Command Line Argument parsing
  - Works with any currency or pool that sends data within the buffer range of 4096 bytes.
  - Asynchronous, supports up to 100 miners at once.
---

### How it's built

* [.NET Core] - The multiplatform framework that this is written in
* [Starksoft.Aspen] - .NET Library for handling TCP sockets over Socks5
* [Newtonsoft.JSON] - .NET Library for handling JSON
---

### Contribution

Contribution is always welcome in the form of a issue or a pull request to improve this software.

---

### Building from source
```bash
git clone https://github.com/Elycin/PoolOverSocks5.git
cd PoolOverSocks5/PoolOverSocks5
dotnet restore
dotnet build
dotnet run 
```

 [.NET Core]: <https://www.microsoft.com/net/learn/get-started/windows>
 [Starksoft.Aspen]: <https://github.com/bentonstark/starksoft-aspen>
 [Newtonsoft.JSON]: <https://www.newtonsoft.com/json>
 
---

### Special Thanks
- [okkero](https://github.com/okkero) - Linq assistance and issue contribution

### Donations
Appreciate my work? thanks!  
If you would like to buy me a drink of some sort you can donate using one of the addresses below!
```
BTC: 1MwzVSXVfm1Gfvtc2n3vqam8434cGA5GgT
ETH: 0xA6b57B0d2b22c6B0031CdC6c3a2953eF93d368e8
LTC: LREiRMXV3BLFAtgXF1uw5QNU2LzGcLbdM1
```
