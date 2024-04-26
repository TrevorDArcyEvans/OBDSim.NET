# OBDSim.NET

## Requirements
* .NET 8 SDK
* git

### Linux
* socat
  ```bash
  sudo apt install -y socat
  ```
 
### Windows
* [com0com](https://com0com.sourceforge.net/)

### Optional
* [PuTTY](https://www.putty.org/)
* [DrawIO](https://github.com/jgraph/drawio-desktop/releases)

## Getting started

```bash
# clone repository
git clone https://github.com/TrevorDArcyEvans/OBDSim.NET.git

# build code
cd OBDSim.NET/src
dotnet build

# run main app
cd OBDSim.NET/bin/Debug/net8.0/
./OBDSim.NET
```

Open [home page](https://localhost:5021/)

## Setting up serial ports

### Linux

<details>

  ```bash
  sudo chmod 777 /dev/ttyV1
  sudo chmod 777 /dev/ttyV2

       socat -d -d PTY,link=/dev/ttyV1,echo=0,raw,unlink-close=0,user=trevorde PTY,link=/dev/ttyV2,echo=0,raw,unlink-close=0,user=trevorde

  sudo socat -d -d PTY,link=/dev/ttyV1,echo=0,unlink-close=0,user=trevorde PTY,link=/dev/ttyV2,echo=0,unlink-close=0,user=trevorde
  ```

 </details>

### Windows

<details>

Use com0com (somehow)

 </details>

## Acknowledgments
* OBD
  * [Get OBD2 Data via ELM327 C#.NET](https://burak.alakus.net/en/2011/07/27/to-get-obd2-data-via-elm327-c/)
  * [OBD2NET](https://github.com/0x8DEADF00D/obd2NET)
  * [OBD-II PIDs](https://en.wikipedia.org/wiki/OBD-II_PIDs)
