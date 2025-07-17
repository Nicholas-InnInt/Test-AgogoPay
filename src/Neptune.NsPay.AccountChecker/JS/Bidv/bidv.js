// RSA with node-forge
"use strict";

// npm install node-forge
const crc = require('crc');
const forge = require("node-forge");
const Buffer = require('buffer/').Buffer
const moment = require('moment')
const crypto = require('crypto').webcrypto;

const G_IS_DEBUG = false
const C_BIDV_PRIVATE_KEY_DEBUG = `-----BEGIN RSA PRIVATE KEY-----
MIICXwIBAAKBgQDXBTOHWD82Djxfc6u06J3RB1qrK90ITvATFsyr864jec2xkd5C
lyb0YW7aXHs8G/eEER9O3KPPslhP1P7QZaWSe9gZB26NLYdDdOyYdg6g2bS0WnyC
Qac0ch/NTtlqk8i2gqVz0CqrBPIhtWiUEBbB2N1h9+/H3Y/9ZoG/tT2ZfQIDAQAB
AoGBAMxY+RvL1mc9KEte1vTbjgC2CIlc6neW7bp2lJVmxTyZ6c60XpLSrAbdAkks
U0JRIe61hxefwV8Gk79rIbBUqgDJpKLmH2zxEFcl80pz7P6I2cRnKg402rusdU4y
UTFvOW/hCQnGkWXTDxkZJu+qnUipt/D6t8HCW1trO58/IENRAkEA/knWeRhg251p
tf/E0m8jPueK+bybXMUPM79AxXvMK1i3n8sORG7GZmR9lsO/ywgMmbfuTpS/XPdl
DzI+Iri8NwJBANh3s3Ga3eD56ZGjFoYMWgTowre2IWUeL2hMFe8e1RJseyK9BZUX
mSupOmd5/1rSOaYQNzKd8gTf02sxA8GCResCQQDqBtSPMCN8Gvw+Fr1ymfwLGZeq
za0CjQ23py2aUpwNzKF6O6vOyVBozdVTmqX52leWZVO6GGWhzsHAYZIT7IazAkEA
0fUYWaJKf0InKBk1aYNldMmGw8WmEnv6o4DY7XvMUvhhXspUNc4TxON5QJB1+1NY
kxe7Uh8cdVnbqGZ8LB79TQJBAMN13MOp7HnEE13d7lJoIqXBRX6B4MhYuSZZ+8Zx
iD43vYnJT/AdIR77iuFvZyrPXV0yXB1mtN6E1krGs9QWjX0=
-----END RSA PRIVATE KEY-----`
const C_BIDV_PUBLIC_KEY_DEBUG = `-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDXBTOHWD82Djxfc6u06J3RB1qrK90ITvATFsyr864jec2xkd5Clyb0YW7aXHs8G/eEER9O3KPPslhP1P7QZaWSe9gZB26NLYdDdOyYdg6g2bS0WnyCQac0ch/NTtlqk8i2gqVz0CqrBPIhtWiUEBbB2N1h9+/H3Y/9ZoG/tT2ZfQIDAQAB
-----END PUBLIC KEY-----`;
const C_BIDV_BROWSERID_DEBUG = "xYxfS3xxrf0362469464bi500328ex72dv";
const C_BIDV_VERSIONID_DEBUG = "2.9.9.61";
const C_BIDV_CLIENTID_DEBUG = null; // null
const C_BIDV_ACCESSKEY_DEBUG = "PT1RUXpSV0x0VlZjdFZWV1lKblFRdDJTVGxqUjZCWGRObGxlVmRrYUhWSE9PbHpTcmRrWjM4bU42bEZOcmRtYVlKM2J5QVRXSWwxVXlZR2RmbGpVNE5XTE81a2VuUkVTYUZsWm1aR05sWjJjdEUzY2tGVU42NVNPS05WVHdrRlJPRlRSVTFrTXJSVVRwOW1hSkZqU0R4VWFqUlZXeWtrTU5obVZ0bDFkalJWVHRaMVZacDNacTFVTlZSa1QxVTBSTmhYUlg1RWJLcFhXemtrTU5sMmJxbDBhc0p6WXBkM1VPWlRTVHBsZUtORVQ2dEdSTk56WTYxVU1ScG5UNDltYUpkSGFZcFZhM2xXU1hKVlZUTmtTcDlVYU5Oell3cFVlbDVTT0tsV1Q0VmxlVmxrU3A5VWFqZGtZb3AwUU1sV1V4WTFTS2wyVHBGRVdsQmpTNVZHSXlWbWNoVm1R";

const C_BIDV_TrustBrowser = {
    browerDefaultString: "bi500328ex72dv",
    clientIdDefaultString: "bi8e00095e76cdv"
};
const C_BIDV_DefaultPublicKey = "LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUF5NmJDOXViNDZWRHdaTDVyd3RiVwoydkJsSHNxR3puNmtyOE9YN2RLbitqSFp4SnhIU09Hd1RscWkrL1FzU1o4d2JVRGt5SzY2YXRZQjRZMDZqMUhTClJpbUxHMnpLSzZCd3F0TXdNMVZCd2VweTZuQitKc2JvYm12REluVS84Y0FyZFFSVk53V01IV3dWMFpCMGEzd3AKRkN2VlN3RjYxekZoNWFHMUdiZnZrYndkaDRicFJhODYwTVR5SzE5K3JSWGJvUk9RbVFZWGZMV2Jyc0k3dmMzUQpGUmZnSElkaDNiYVZkMG1qbWdNaEU5eVh3enJvT3hkNDE4YVdVUTllU1kxeG1FbVg5UXluRzlkWUJNbC96enVTCm1NNkNmSndLZHNzd0tGMHZtaFJTTE9CditqL2pBQkFEY25yY0loY0JTM0VuVHRTWERRUG4vTy9vc3F2UnU1cQp4dlFJREFRQUIKLS0tLS1FTkQgUFVCTElDIEtFWS0tLS0t"
const C_BIDV_PRIVATE_KEY = `-----BEGIN RSA PRIVATE KEY-----
MIICXgIBAAKBgQC65bjJEsapGztqjeZlovccRf4v894w3vyNz7fe8LrdSXixrD5X
j486AI4yutCBjPPj1PrzSxqQgORVRbkxC40ium1G6JCRMHQ2G1Dyh3tphXH+w0C8
cUO1Dv+GomU/+fRv+yLemB5jd1qkZdrllnOgmptLcjyIiQ/iSHPccNzzcQIDAQAB
AoGBAIjmXm2CwFv8Y5BQcrR+I63lIyTy6A06ORqTTacTBH21JKBYf63ZrVsaLw6/
wp0ypy96wXtWxxWUJCzGDrn7MRw/p5vgfdDSfzyyVdHjB+N+wd0ppbUWLga2cWS+
r/v25bInoL100BO6mkkvxD2yPurbjthozO01IuXds9OPyBNBAkEA3EgmxT9ayWGp
kePGnZ+EGoXADrU5gpvmGKMUDhfRzp//BOXtYEh+TwpVEdCWQ7K2aleMBaiqXHrK
zXg8F83a2QJBANkzybARHxt9nTb2hBuX+aAdiUauBXJ/RkqaOg/leD5WcjPisflH
eZ+r/1jVCG6aeGJv3w9f9sIHEFeJnu50DlkCQQCp5qygroDjmoQjlj93C7XkjwzX
S0gUSRJsJjwtsomMiTM1H/K6tK9XX2zF1NBRXuH7m5LQOotL0Rni6L3QzmHhAkAL
U/Ie5qWyr1h1t2GodsKkISY5s3XBRPYLigOhNJyS07tdDWOu1pV3SGcm4OVxr0i8
CY2epaie8fNePWU2loLxAkEAhTCg263huDiIZ0Qqr9ZWL0+9L7n5udKywMclwFTe
RtYTqnro8OTN/J/ftqBBPiCACbQzsSha8IR+bjsUtj6WuA==
-----END RSA PRIVATE KEY-----`
const C_BIDV_PUBLIC_KEY = `-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC65bjJEsapGztqjeZlovccRf4v
894w3vyNz7fe8LrdSXixrD5Xj486AI4yutCBjPPj1PrzSxqQgORVRbkxC40ium1G
6JCRMHQ2G1Dyh3tphXH+w0C8cUO1Dv+GomU/+fRv+yLemB5jd1qkZdrllnOgmptL
cjyIiQ/iSHPccNzzcQIDAQAB
-----END PUBLIC KEY-----`;

/// bidv localStorage name
const C_BIDV_BrowserId = "5d678a4961ea2";
const C_BIDV_VersionId = "a084ac21693930";
const C_BIDV_ClientId = "aa8f72993c4c"; // null
/// bidv sessionStorage name
const C_BIDV_AccessKeyId = "5d678a4961ea2aa583868e22231f7857";

module.exports = class BIDV {

    encryptRequest_getinternalbeneficiary() { 
        // "mid": 200,
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    browserId: C_BIDV_BROWSERID_DEBUG,
                    clientId: C_BIDV_CLIENTID_DEBUG,
                    // accessKeyId: C_BIDV_ACCESSKEY_DEBUG, // decryption using runtime crypto, retrieve via httprequest
                    appVersionId: C_BIDV_VERSIONID_DEBUG,
                    encrypt: {
                        accNo: "8873266080",
                    }
                }
                // console.log(JSON.stringify(data))
            } else {
                data = JSON.parse(process.argv[2])
            }

            // data = "{\"browserId\":null,\"clientId\":null,\"accessKeyId\":\"PT1RVUNWalZvaDFjd1kyVHRzVWRtNVVhekVWVXpjMVRVVlZVQlZXTkRaelNHbGpXblp6ZHkxMFRCWlVVUUJEUnhOR1M1TTFSNnRXTTJ3a2RCaERkcmwwUlQ5bFpsWkhVRUJsU3lFRlpNSkVid3BtZExsRWRWNVNPS05WVHdrRlJPRlRSVTFrTXJSVVRwOW1hSkZqU0R4VWFKZGxUeFVsTU9wbVFVNUViV2QxVDBVRlZhbG1TWHBsTUZSa1RwSkZWT0ZUU3RwRmFTMVdUMWtsYU5sMmJxbDBhc0p6WXBkM1VPWlRTVHBsZUtORVQ1MUVST0Z6WTYxVU1ScG5UNDltYUpkSGFZcFZhM2xXU1hKVlZUTmtTcDlVYU5Oell3cFVlbDVTT0tsV1Q0VmxlVmxrU3A5VWFqZGtZb3AwUU1sV1V4WTFTS2wyVHBGRVdsQmpTNVZHSXlWbWNoVm1R\",\"appVersionId\":\"2.9.9.61\",\"encrypt\":{\"accNo\":\"8864181537\",\"moreRecord\":\"N\",\"nextRunbal\":\"\",\"postingDate\":\"\",\"postingOrder\":\"\",\"fileIndicator\":\"\"}}"
            // data = JSON.parse(data)

            const clientPublicKey = (G_IS_DEBUG ? C_BIDV_PUBLIC_KEY_DEBUG : C_BIDV_PUBLIC_KEY).replace(/(-|(BEGIN|END) PUBLIC KEY|\r|\n|\s)/gi, "");

            // BIDV each session generating new Keys
            // this script use fix keys
            // var keyPair = forge.pki.rsa.generateKeyPair({ bits: 1024, workers: 1 })
            // var privateKey = forge.pki.privateKeyToPem(keyPair.privateKey);
            // var publicKey = forge.pki.publicKeyToPem(keyPair.publicKey);
            // const clientPublicKey = publicKey.replace(/(-|(BEGIN|END) PUBLIC KEY|\r|\n|\s)/gi, "");

            data.encrypt = {
                mid: 200,
                DT: "WINDOWS",
                PM: "Chrome",
                OV: "135.0.0.0",
                appVersion: data.appVersionId,
                E: data.browserId ? data.browserId.replace(C_BIDV_TrustBrowser.browerDefaultString, "") : "",
                clientId: data.clientId ? data.clientId.replace(C_BIDV_TrustBrowser.clientIdDefaultString, "") : "",
                clientPubKey: clientPublicKey,
                ...data.encrypt,
            }
            // console.log(JSON.stringify(data.encrypt))

            // Encrypt formula
            const t = forge.random.getBytesSync(32)
            const i = forge.random.getBytesSync(16)
            const o = forge.cipher.createCipher("AES-CTR", t);
            o.start({
                iv: i
            })
            o.update(forge.util.createBuffer(forge.util.encodeUtf8(JSON.stringify(data.encrypt))))
            o.finish();
            const s = Buffer.concat([Buffer.from(i, "binary"), Buffer.from(o.output.data, "binary")])
            const r = forge.pki.publicKeyFromPem(forge.util.decode64(C_BIDV_DefaultPublicKey)).encrypt(forge.util.encode64(t));

            // Encrypted result
            var encrypted = {
                d: s.toString("base64"),
                k: forge.util.encode64(r)
            }
            // console.log(encrypted)

            // // Headers
            var x_request_id = `${moment().format('HHmmssSSS')}${this.randomNumberString(6)}`
            // var token = this.decryptText(data.accessKeyId) // decryption using runtime crypto, retrieve via httprequest
            var result = {
                x_request_id: x_request_id,
                // token: token,
                body: encrypted,
            }
            console.log(JSON.stringify(result))

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    encryptRequest_getexternalbeneficiary() { 
        // "mid": 201,
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    browserId: C_BIDV_BROWSERID_DEBUG,
                    clientId: C_BIDV_CLIENTID_DEBUG,
                    // accessKeyId: C_BIDV_ACCESSKEY_DEBUG, // decryption using runtime crypto, retrieve via httprequest
                    appVersionId: C_BIDV_VERSIONID_DEBUG,
                    encrypt: {
                        accNo: "104882287128",
                        bankCode247: "970415",
                    }
                }
                // console.log(JSON.stringify(data))
            } else {
                data = JSON.parse(process.argv[2])
            }

            // data = "{\"browserId\":null,\"clientId\":null,\"accessKeyId\":\"PT1RVUNWalZvaDFjd1kyVHRzVWRtNVVhekVWVXpjMVRVVlZVQlZXTkRaelNHbGpXblp6ZHkxMFRCWlVVUUJEUnhOR1M1TTFSNnRXTTJ3a2RCaERkcmwwUlQ5bFpsWkhVRUJsU3lFRlpNSkVid3BtZExsRWRWNVNPS05WVHdrRlJPRlRSVTFrTXJSVVRwOW1hSkZqU0R4VWFKZGxUeFVsTU9wbVFVNUViV2QxVDBVRlZhbG1TWHBsTUZSa1RwSkZWT0ZUU3RwRmFTMVdUMWtsYU5sMmJxbDBhc0p6WXBkM1VPWlRTVHBsZUtORVQ1MUVST0Z6WTYxVU1ScG5UNDltYUpkSGFZcFZhM2xXU1hKVlZUTmtTcDlVYU5Oell3cFVlbDVTT0tsV1Q0VmxlVmxrU3A5VWFqZGtZb3AwUU1sV1V4WTFTS2wyVHBGRVdsQmpTNVZHSXlWbWNoVm1R\",\"appVersionId\":\"2.9.9.61\",\"encrypt\":{\"accNo\":\"8864181537\",\"moreRecord\":\"N\",\"nextRunbal\":\"\",\"postingDate\":\"\",\"postingOrder\":\"\",\"fileIndicator\":\"\"}}"
            // data = JSON.parse(data)

            const clientPublicKey = (G_IS_DEBUG ? C_BIDV_PUBLIC_KEY_DEBUG : C_BIDV_PUBLIC_KEY).replace(/(-|(BEGIN|END) PUBLIC KEY|\r|\n|\s)/gi, "");

            // BIDV each session generating new Keys
            // this script use fix keys
            // var keyPair = forge.pki.rsa.generateKeyPair({ bits: 1024, workers: 1 })
            // var privateKey = forge.pki.privateKeyToPem(keyPair.privateKey);
            // var publicKey = forge.pki.publicKeyToPem(keyPair.publicKey);
            // const clientPublicKey = publicKey.replace(/(-|(BEGIN|END) PUBLIC KEY|\r|\n|\s)/gi, "");

            data.encrypt = {
                mid: 201,
                DT: "WINDOWS",
                PM: "Chrome",
                OV: "135.0.0.0",
                appVersion: data.appVersionId,
                E: data.browserId ? data.browserId.replace(C_BIDV_TrustBrowser.browerDefaultString, "") : "",
                clientId: data.clientId ? data.clientId.replace(C_BIDV_TrustBrowser.clientIdDefaultString, "") : "",
                clientPubKey: clientPublicKey,
                ...data.encrypt,
            }
            // console.log(JSON.stringify(data.encrypt))

            // Encrypt formula
            const t = forge.random.getBytesSync(32)
            const i = forge.random.getBytesSync(16)
            const o = forge.cipher.createCipher("AES-CTR", t);
            o.start({
                iv: i
            })
            o.update(forge.util.createBuffer(forge.util.encodeUtf8(JSON.stringify(data.encrypt))))
            o.finish();
            const s = Buffer.concat([Buffer.from(i, "binary"), Buffer.from(o.output.data, "binary")])
            const r = forge.pki.publicKeyFromPem(forge.util.decode64(C_BIDV_DefaultPublicKey)).encrypt(forge.util.encode64(t));

            // Encrypted result
            var encrypted = {
                d: s.toString("base64"),
                k: forge.util.encode64(r)
            }
            // console.log(encrypted)

            // // Headers
            var x_request_id = `${moment().format('HHmmssSSS')}${this.randomNumberString(6)}`
            // var token = this.decryptText(data.accessKeyId) // decryption using runtime crypto, retrieve via httprequest
            var result = {
                x_request_id: x_request_id,
                // token: token,
                body: encrypted,
            }
            console.log(JSON.stringify(result))

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    encryptRequest_getaccountdetail() {
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    browserId: C_BIDV_BROWSERID_DEBUG,
                    clientId: C_BIDV_CLIENTID_DEBUG,
                    // accessKeyId: C_BIDV_ACCESSKEY_DEBUG, // decryption using runtime crypto, retrieve via httprequest
                    appVersionId: C_BIDV_VERSIONID_DEBUG,
                    encrypt: null
                }
                // console.log(JSON.stringify(data))
            } else {
                data = JSON.parse(process.argv[2])
            }

            // data = "{\"browserId\":null,\"clientId\":null,\"accessKeyId\":\"PT1RVUNWalZvaDFjd1kyVHRzVWRtNVVhekVWVXpjMVRVVlZVQlZXTkRaelNHbGpXblp6ZHkxMFRCWlVVUUJEUnhOR1M1TTFSNnRXTTJ3a2RCaERkcmwwUlQ5bFpsWkhVRUJsU3lFRlpNSkVid3BtZExsRWRWNVNPS05WVHdrRlJPRlRSVTFrTXJSVVRwOW1hSkZqU0R4VWFKZGxUeFVsTU9wbVFVNUViV2QxVDBVRlZhbG1TWHBsTUZSa1RwSkZWT0ZUU3RwRmFTMVdUMWtsYU5sMmJxbDBhc0p6WXBkM1VPWlRTVHBsZUtORVQ1MUVST0Z6WTYxVU1ScG5UNDltYUpkSGFZcFZhM2xXU1hKVlZUTmtTcDlVYU5Oell3cFVlbDVTT0tsV1Q0VmxlVmxrU3A5VWFqZGtZb3AwUU1sV1V4WTFTS2wyVHBGRVdsQmpTNVZHSXlWbWNoVm1R\",\"appVersionId\":\"2.9.9.61\",\"encrypt\":{\"accNo\":\"8864181537\",\"moreRecord\":\"N\",\"nextRunbal\":\"\",\"postingDate\":\"\",\"postingOrder\":\"\",\"fileIndicator\":\"\"}}"
            // data = JSON.parse(data)

            const clientPublicKey = (G_IS_DEBUG ? C_BIDV_PUBLIC_KEY_DEBUG : C_BIDV_PUBLIC_KEY).replace(/(-|(BEGIN|END) PUBLIC KEY|\r|\n|\s)/gi, "");

            // BIDV each session generating new Keys
            // this script use fix keys
            // var keyPair = forge.pki.rsa.generateKeyPair({ bits: 1024, workers: 1 })
            // var privateKey = forge.pki.privateKeyToPem(keyPair.privateKey);
            // var publicKey = forge.pki.publicKeyToPem(keyPair.publicKey);
            // const clientPublicKey = publicKey.replace(/(-|(BEGIN|END) PUBLIC KEY|\r|\n|\s)/gi, "");

            data.encrypt = {
                accType: "D",
                mid: 10,
                DT: "WINDOWS",
                PM: "Chrome",
                OV: "135.0.0.0",
                appVersion: data.appVersionId,
                E: data.browserId ? data.browserId.replace(C_BIDV_TrustBrowser.browerDefaultString, "") : "",
                clientId: data.clientId ? data.clientId.replace(C_BIDV_TrustBrowser.clientIdDefaultString, "") : "",
                clientPubKey: clientPublicKey,
                ...data.encrypt,
            }
            // console.log(JSON.stringify(data.encrypt))

            // Encrypt formula
            const t = forge.random.getBytesSync(32)
            const i = forge.random.getBytesSync(16)
            const o = forge.cipher.createCipher("AES-CTR", t);
            o.start({
                iv: i
            })
            o.update(forge.util.createBuffer(forge.util.encodeUtf8(JSON.stringify(data.encrypt))))
            o.finish();
            const s = Buffer.concat([Buffer.from(i, "binary"), Buffer.from(o.output.data, "binary")])
            const r = forge.pki.publicKeyFromPem(forge.util.decode64(C_BIDV_DefaultPublicKey)).encrypt(forge.util.encode64(t));

            // Encrypted result
            var encrypted = {
                d: s.toString("base64"),
                k: forge.util.encode64(r)
            }
            // console.log(encrypted)

            // // Headers
            var x_request_id = `${moment().format('HHmmssSSS')}${this.randomNumberString(6)}`
            // var token = this.decryptText(data.accessKeyId) // decryption using runtime crypto, retrieve via httprequest
            var result = {
                x_request_id: x_request_id,
                // token: token,
                body: encrypted,
            }
            console.log(JSON.stringify(result))

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    encryptRequest_transhist() {
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    browserId: C_BIDV_BROWSERID_DEBUG,
                    clientId: C_BIDV_CLIENTID_DEBUG,
                    // accessKeyId: C_BIDV_ACCESSKEY_DEBUG,
                    appVersionId: C_BIDV_VERSIONID_DEBUG,
                    encrypt: {
                        accNo: "8864181537",
                        moreRecord: "N", // "Y", last response: moreRecord
                        nextRunbal: "", // "2057551", this.transactions[this.transactions.length - 1].balance,
                        postingDate: "", // "210425", this.transactions[this.transactions.length - 1].postingDate,
                        postingOrder: "", // "IFX|22|MDAyNTU1OTY4NjY3MzE3NDgxOTYwMQ==", this.transactions[this.transactions.length - 1].postingOrder
                        fileIndicator: "", // "", this.transactions[this.transactions.length - 1].fileIndicator
                    }
                }
                // console.log(JSON.stringify(data))
            } else {
                data = JSON.parse(process.argv[2])
            }

            // data = "{\"browserId\":null,\"clientId\":null,\"accessKeyId\":\"PT1RVUNWalZvaDFjd1kyVHRzVWRtNVVhekVWVXpjMVRVVlZVQlZXTkRaelNHbGpXblp6ZHkxMFRCWlVVUUJEUnhOR1M1TTFSNnRXTTJ3a2RCaERkcmwwUlQ5bFpsWkhVRUJsU3lFRlpNSkVid3BtZExsRWRWNVNPS05WVHdrRlJPRlRSVTFrTXJSVVRwOW1hSkZqU0R4VWFKZGxUeFVsTU9wbVFVNUViV2QxVDBVRlZhbG1TWHBsTUZSa1RwSkZWT0ZUU3RwRmFTMVdUMWtsYU5sMmJxbDBhc0p6WXBkM1VPWlRTVHBsZUtORVQ1MUVST0Z6WTYxVU1ScG5UNDltYUpkSGFZcFZhM2xXU1hKVlZUTmtTcDlVYU5Oell3cFVlbDVTT0tsV1Q0VmxlVmxrU3A5VWFqZGtZb3AwUU1sV1V4WTFTS2wyVHBGRVdsQmpTNVZHSXlWbWNoVm1R\",\"appVersionId\":\"2.9.9.61\",\"encrypt\":{\"accNo\":\"8864181537\",\"moreRecord\":\"N\",\"nextRunbal\":\"\",\"postingDate\":\"\",\"postingOrder\":\"\",\"fileIndicator\":\"\"}}"
            // data = JSON.parse(data)

            const clientPublicKey = (G_IS_DEBUG ? C_BIDV_PUBLIC_KEY_DEBUG : C_BIDV_PUBLIC_KEY).replace(/(-|(BEGIN|END) PUBLIC KEY|\r|\n|\s)/gi, "");

            // BIDV each session generating new Keys
            // this script use fix keys
            // var keyPair = forge.pki.rsa.generateKeyPair({ bits: 1024, workers: 1 })
            // var privateKey = forge.pki.privateKeyToPem(keyPair.privateKey);
            // var publicKey = forge.pki.publicKeyToPem(keyPair.publicKey);
            // const clientPublicKey = publicKey.replace(/(-|(BEGIN|END) PUBLIC KEY|\r|\n|\s)/gi, "");

            data.encrypt = {
                accType: "D",
                mid: 12,
                DT: "WINDOWS",
                PM: "Chrome",
                OV: "135.0.0.0",
                appVersion: data.appVersionId,
                E: data.browserId ? data.browserId.replace(C_BIDV_TrustBrowser.browerDefaultString, "") : "",
                clientId: data.clientId ? data.clientId.replace(C_BIDV_TrustBrowser.clientIdDefaultString, "") : "",
                clientPubKey: clientPublicKey,
                ...data.encrypt,
            }
            // console.log(JSON.stringify(data.encrypt))

            // Encrypt formula
            const t = forge.random.getBytesSync(32)
            const i = forge.random.getBytesSync(16)
            const o = forge.cipher.createCipher("AES-CTR", t);
            o.start({
                iv: i
            })
            o.update(forge.util.createBuffer(forge.util.encodeUtf8(JSON.stringify(data.encrypt))))
            o.finish();
            const s = Buffer.concat([Buffer.from(i, "binary"), Buffer.from(o.output.data, "binary")])
            const r = forge.pki.publicKeyFromPem(forge.util.decode64(C_BIDV_DefaultPublicKey)).encrypt(forge.util.encode64(t));

            // Encrypted result
            var encrypted = {
                d: s.toString("base64"),
                k: forge.util.encode64(r)
            }
            // console.log(encrypted)

            // // Headers
            var x_request_id = `${moment().format('HHmmssSSS')}${this.randomNumberString(6)}`
            // var token = this.decryptText(data.accessKeyId) // decryption using runtime crypto, retrieve via httprequest
            var result = {
                x_request_id: x_request_id,
                // token: token,
                body: encrypted,
            }
            console.log(JSON.stringify(result))

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    decryptResponse() {
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    "userJwt": null,
                    "k": "GpU1BJYjj8mwp8RddfQI80kiBNaSyTvGaF1C7raqgB4VK7R2OYvS6vqWEBb3X6EXqMLX24FiHqBMuyqL6naN4iwl5meq2BaWGrkfMvsZ7s/EYgZNpcbAJy3loTxTBvRy1NM4uuahehYkzexI7yoXZ1LGeNjYMw2nlur/QpMeDVw=",
                    "d": "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",
                    "file": null
                }
                // console.log(JSON.stringify(data))
            } else {
                data = JSON.parse(process.argv[2])
            }

            // Decrypt formula
            const { k: t, d: i } = data
            const o = forge.pki.privateKeyFromPem(G_IS_DEBUG ? C_BIDV_PRIVATE_KEY_DEBUG : C_BIDV_PRIVATE_KEY)
            const s = forge.util.decodeUtf8(o.decrypt(forge.util.decode64(t)))
            const r = Buffer.from(i, "base64")
            const c = r.slice(0, 16)
            const h = r.slice(16)
            const l = forge.cipher.createDecipher("AES-CTR", Buffer.from(s, "base64").toString("binary"));
            l.start({
                iv: c.toString("binary")
            })
            l.update(forge.util.createBuffer(h))
            l.finish()

            // Decrypted
            var result = forge.util.decodeUtf8(l.output.data)
            console.log(result)

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    decryptText(e) {
        try {
            // window.atob ~ decodeBase64
            return e && "string" == typeof e ? (e = decodeURIComponent(escape(this.decodeBase64(e))),
                e = Array.from(e).reverse().join(""),
                decodeURIComponent(escape(this.decodeBase64(e)))) : e
        } catch (i) {
            return console.log(i),
                e
        }
    }

    encodeBase64(data) {
        return Buffer.from(data).toString('base64');
    }
    decodeBase64(data) {
        return Buffer.from(data, 'base64').toString('ascii');
    }

    randomString(e = 10, t = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") {
        const n = new Uint32Array(1);
        let a = "";
        const o = t
            , s = o.length;
        for (let r = 0; r < e; r++) {
            const e = Number(crypto.getRandomValues(n).toString()) % 10 / 10;
            a += o.charAt(Math.floor(e * s))
        }
        return a
    }

    randomNumberString(e, t = "0123456789") {
        return this.randomString(e, t)
    }
}






