// RSA with node-forge
"use strict";

const NodeRSA = require('node-rsa');

const G_IS_DEBUG = false
const C_VTB_HASH = [1732584193, 4023233417, 2562383102, 271733878];
const C_VTB_PUBLIC_KEY = "-----BEGIN PUBLIC KEY-----\n" +
    "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLenQHmHpaqYX4IrRVM8H1uB21\n" +
    "xWuY+clsvn79pMUYR2KwIEfeHcnZFFshjDs3D2ae4KprjkOFZPYzEWzakg2nOIUV\n" +
    "WO+Q6RlAU1+1fxgTvEXi4z7yi+n0Zs0puOycrm8i67jsQfHi+HgdMxCaKzHvbECr\n" +
    "+JWnLxnEl6615hEeMQIDAQAB\n" +
    "-----END PUBLIC KEY-----";

/// vtb sessionStorage
const C_VTB_SESSIONID_DEBUG = "NZ9YHGR1RR"

/// vtb sessionStorage name
const C_VTB_SESSIONID = "session" // {"session":"NZ9YHGR1RR"}

module.exports = class VTB {

    _HE = []

    encryptRequest_internaltransfer() {
        // // https://api-ipay.vietinbank.vn/ipay/wa/makeInternalTransfer
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    sessionId: C_VTB_SESSIONID_DEBUG,
                    encrypt: {
                        accountNumber: "104882287128", // to account no.
                    }
                }
            } else {
                data = JSON.parse(process.argv[2])
            }
            // console.log(data)

            data.encrypt = {
                "formAction": "inquiryToAcctDetail",
                "lang": "en",
                "sessionId": data.sessionId,
                ...data.encrypt,
            }

            var requestId = this._genRequestId();
            data.encrypt["requestId"] = requestId;
            // console.log(data)

            var parseSignature = null;
            Object.keys(data.encrypt).sort().forEach(function (key) {
                if (parseSignature == null)
                    parseSignature = key + "=" + encodeURI(data.encrypt[key])
                else
                    parseSignature += "&" + key + "=" + encodeURI(data.encrypt[key])
            });
            // console.log(parseSignature)

            var signatureParse = this._signatureParse(parseSignature)

            var signature = this._genSignature(signatureParse);
            data.encrypt["signature"] = signature;

            // const NodeRSA = require('node-rsa');
            let key_public = new NodeRSA(C_VTB_PUBLIC_KEY);

            // Public key for encryption
            var result = key_public.encrypt(JSON.stringify(data.encrypt), "base64")
            console.log(result)

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    encryptRequest_napastransfer() {
        // https://api-ipay.vietinbank.vn/ipay/wa/napasTransfer
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    sessionId: C_VTB_SESSIONID_DEBUG,
                    encrypt: {
                        fromAccount: "100883370218", // from account no.
                        beneficiaryType: "account",
                        beneficiaryAccount: "8864181537", // to account no
                        beneficiaryBin: "970418",
                    }
                }
            } else {
                data = JSON.parse(process.argv[2])
            }
            // console.log(data)

            data.encrypt = {
                "formAction": "validateToAccount",
                "lang": "en",
                "sessionId": data.sessionId,
                ...data.encrypt,
            }

            var requestId = this._genRequestId();
            data.encrypt["requestId"] = requestId;
            // console.log(data)

            var parseSignature = null;
            Object.keys(data.encrypt).sort().forEach(function (key) {
                if (parseSignature == null)
                    parseSignature = key + "=" + encodeURI(data.encrypt[key])
                else
                    parseSignature += "&" + key + "=" + encodeURI(data.encrypt[key])
            });
            // console.log(parseSignature)

            var signatureParse = this._signatureParse(parseSignature)

            var signature = this._genSignature(signatureParse);
            data.encrypt["signature"] = signature;

            // const NodeRSA = require('node-rsa');
            let key_public = new NodeRSA(C_VTB_PUBLIC_KEY);

            // Public key for encryption
            var result = key_public.encrypt(JSON.stringify(data.encrypt), "base64")
            console.log(result)

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    encryptRequest_getaccountdetails() {
        // https://api-ipay.vietinbank.vn/ipay/wa/getAccountDetails
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    sessionId: C_VTB_SESSIONID_DEBUG,
                    encrypt: {
                        accountNumber: "100883370218",
                    }
                }
            } else {
                data = JSON.parse(process.argv[2])
            }
            // console.log(data)

            data.encrypt = {
                "accountType": "DDA",
                "lang": "en",
                "sessionId": data.sessionId,
                ...data.encrypt,
            }

            var requestId = this._genRequestId();
            data.encrypt["requestId"] = requestId;
            // console.log(data)

            var parseSignature = null;
            Object.keys(data.encrypt).sort().forEach(function (key) {
                if (parseSignature == null)
                    parseSignature = key + "=" + encodeURI(data.encrypt[key])
                else
                    parseSignature += "&" + key + "=" + encodeURI(data.encrypt[key])
            });
            // console.log(parseSignature)

            var signatureParse = this._signatureParse(parseSignature)

            var signature = this._genSignature(signatureParse);
            data.encrypt["signature"] = signature;

            // const NodeRSA = require('node-rsa');
            let key_public = new NodeRSA(C_VTB_PUBLIC_KEY);

            // Public key for encryption
            var result = key_public.encrypt(JSON.stringify(data.encrypt), "base64")
            console.log(result)

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    encryptRequest_transhist() {
        // https://api-ipay.vietinbank.vn/ipay/wa/getHistTransactions
        try {
            var data = null

            if (G_IS_DEBUG) {
                data = {
                    sessionId: C_VTB_SESSIONID_DEBUG,
                    encrypt: {
                        accountNumber: "100883370218",
                        endDate: "2025-03-05",
                        startDate: "2025-02-03",
                    }
                }
            } else {
                data = JSON.parse(process.argv[2])
            }
            // console.log(data)

            data.encrypt = {
                "lang": "en",
                "maxResult": "999999999",
                "pageNumber": 0,
                "searchFromAmt": "",
                "searchKey": "",
                "searchToAmt": "",
                "tranType": "",
                "sessionId": data.sessionId,
                ...data.encrypt,
            }

            var requestId = this._genRequestId();
            data.encrypt["requestId"] = requestId;
            // console.log(data)

            var parseSignature = null;
            Object.keys(data.encrypt).sort().forEach(function (key) {
                if (parseSignature == null)
                    parseSignature = key + "=" + encodeURI(data.encrypt[key])
                else
                    parseSignature += "&" + key + "=" + encodeURI(data.encrypt[key])
            });
            // console.log(parseSignature)

            var signatureParse = this._signatureParse(parseSignature)

            var signature = this._genSignature(signatureParse);
            data.encrypt["signature"] = signature;

            // const NodeRSA = require('node-rsa');
            let key_public = new NodeRSA(C_VTB_PUBLIC_KEY);

            // Public key for encryption
            var result = key_public.encrypt(JSON.stringify(data.encrypt), "base64")
            console.log(result)

            console.log("done!")
            return
        } catch (error) {
            console.log("error! ", error.message)
        }
    }

    _genRequestId() {
        for (var t = "".concat("ABCDEFGHIJKLMNOPQRSTUVWXYZ").concat("0123456789"), a = "", r = 0; r < 12; r++)
            a += t[Math.floor(Math.random() * t.length)];
        return "".concat(a, "|").concat((new Date).getTime())
    }

    _signatureParse(q) {
        for (var ne = q.length, oe = [], ie = 0; ie < ne; ie++)
            oe[ie >>> 2] |= (255 & q.charCodeAt(ie)) << 24 - ie % 4 * 8;

        return { "sigBytes": ne, "words": oe }
    }

    _signatureStringtify(q) {
        for (var ne = q.words, oe = q.sigBytes, ie = [], se = 0; se < oe; se++) {
            var le = ne[se >>> 2] >>> 24 - se % 4 * 8 & 255;
            ie.push((le >>> 4).toString(16)),
                ie.push((15 & le).toString(16))
        }
        return ie.join("")
    }

    _genSignature(d) {
        this._HE = this._signatureFinalizeHash()

        var ne = d
            , oe = ne.words
            , ie = 8 * d.sigBytes
            , se = 8 * ne.sigBytes;
        oe[se >>> 5] |= 128 << 24 - se % 32;
        var le = Math.floor(ie / 4294967296)
            , pe = ie;
        oe[15 + (se + 64 >>> 9 << 4)] = 16711935 & (le << 8 | le >>> 24) | 4278255360 & (le << 24 | le >>> 8),
            oe[14 + (se + 64 >>> 9 << 4)] = 16711935 & (pe << 8 | pe >>> 24) | 4278255360 & (pe << 24 | pe >>> 8),
            ne.sigBytes = 4 * (oe.length + 1);

        let r = { "sigBytes": d.sigBytes, "words": d.words, "blockSize": 16, "minBufferSize": 0 }

        // process
        let process = this._signatureFinalizeProcess(r);

        // after process
        this._signatureFinalizeAfterProcess(process.hash);
        let signatureString = this._signatureStringtify(process.hash);

        return signatureString
    }

    _signatureFinalizeHash() {
        var he = []
        for (var ne = 0; ne < 64; ne++)
            he[ne] = 4294967296 * Math.abs(Math.sin(ne + 1)) | 0
        return he
    }

    _signatureFinalizeProcess(d) {
        var ne = undefined
        var oe,
            ie = d,
            se = ie.words,
            le = ie.sigBytes,
            pe = d.blockSize,
            he = le / (4 * pe),
            ge = (he = ne ? Math.ceil(he) : Math.max((0 | he) - d.minBufferSize, 0)) * pe,
            ve = Math.min(4 * ge, le);
        if (ge) {
            var le_hash = C_VTB_HASH;
            for (var me = 0; me < ge; me += pe)
                this._signatureFinalizeProcessBlock(se, me, le_hash);
            oe = se.splice(0, ge),
                ie.sigBytes -= ve
        }
        return { "sigBytes": ve, "words": oe, "hash": { "sigBytes": 16, "words": le_hash } }
    }

    _signatureFinalizeProcessBlock(q, ne, le_hash) {
        for (var oe = 0; oe < 16; oe++) {
            var ie = ne + oe
                , se = q[ie];
            q[ie] = 16711935 & (se << 8 | se >>> 24) | 4278255360 & (se << 24 | se >>> 8)
        }
        var le = le_hash
            , pe = q[ne + 0]
            , ge = q[ne + 1]
            , ye = q[ne + 2]
            , ve = q[ne + 3]
            , me = q[ne + 4]
            , _e = q[ne + 5]
            , we = q[ne + 6]
            , Se = q[ne + 7]
            , Ae = q[ne + 8]
            , Oe = q[ne + 9]
            , ke = q[ne + 10]
            , Te = q[ne + 11]
            , je = q[ne + 12]
            , Me = q[ne + 13]
            , Re = q[ne + 14]
            , Be = q[ne + 15]
            , Ne = le[0]
            , Fe = le[1]
            , Ue = le[2]
            , We = le[3];
        Ne = this._FF(Ne, Fe, Ue, We, pe, 7, this._HE[0]),
            We = this._FF(We, Ne, Fe, Ue, ge, 12, this._HE[1]),
            Ue = this._FF(Ue, We, Ne, Fe, ye, 17, this._HE[2]),
            Fe = this._FF(Fe, Ue, We, Ne, ve, 22, this._HE[3]),
            Ne = this._FF(Ne, Fe, Ue, We, me, 7, this._HE[4]),
            We = this._FF(We, Ne, Fe, Ue, _e, 12, this._HE[5]),
            Ue = this._FF(Ue, We, Ne, Fe, we, 17, this._HE[6]),
            Fe = this._FF(Fe, Ue, We, Ne, Se, 22, this._HE[7]),
            Ne = this._FF(Ne, Fe, Ue, We, Ae, 7, this._HE[8]),
            We = this._FF(We, Ne, Fe, Ue, Oe, 12, this._HE[9]),
            Ue = this._FF(Ue, We, Ne, Fe, ke, 17, this._HE[10]),
            Fe = this._FF(Fe, Ue, We, Ne, Te, 22, this._HE[11]),
            Ne = this._FF(Ne, Fe, Ue, We, je, 7, this._HE[12]),
            We = this._FF(We, Ne, Fe, Ue, Me, 12, this._HE[13]),
            Ue = this._FF(Ue, We, Ne, Fe, Re, 17, this._HE[14]),
            Ne = this._GG(Ne, Fe = this._FF(Fe, Ue, We, Ne, Be, 22, this._HE[15]), Ue, We, ge, 5, this._HE[16]),
            We = this._GG(We, Ne, Fe, Ue, we, 9, this._HE[17]),
            Ue = this._GG(Ue, We, Ne, Fe, Te, 14, this._HE[18]),
            Fe = this._GG(Fe, Ue, We, Ne, pe, 20, this._HE[19]),
            Ne = this._GG(Ne, Fe, Ue, We, _e, 5, this._HE[20]),
            We = this._GG(We, Ne, Fe, Ue, ke, 9, this._HE[21]),
            Ue = this._GG(Ue, We, Ne, Fe, Be, 14, this._HE[22]),
            Fe = this._GG(Fe, Ue, We, Ne, me, 20, this._HE[23]),
            Ne = this._GG(Ne, Fe, Ue, We, Oe, 5, this._HE[24]),
            We = this._GG(We, Ne, Fe, Ue, Re, 9, this._HE[25]),
            Ue = this._GG(Ue, We, Ne, Fe, ve, 14, this._HE[26]),
            Fe = this._GG(Fe, Ue, We, Ne, Ae, 20, this._HE[27]),
            Ne = this._GG(Ne, Fe, Ue, We, Me, 5, this._HE[28]),
            We = this._GG(We, Ne, Fe, Ue, ye, 9, this._HE[29]),
            Ue = this._GG(Ue, We, Ne, Fe, Se, 14, this._HE[30]),
            Ne = this._HH(Ne, Fe = this._GG(Fe, Ue, We, Ne, je, 20, this._HE[31]), Ue, We, _e, 4, this._HE[32]),
            We = this._HH(We, Ne, Fe, Ue, Ae, 11, this._HE[33]),
            Ue = this._HH(Ue, We, Ne, Fe, Te, 16, this._HE[34]),
            Fe = this._HH(Fe, Ue, We, Ne, Re, 23, this._HE[35]),
            Ne = this._HH(Ne, Fe, Ue, We, ge, 4, this._HE[36]),
            We = this._HH(We, Ne, Fe, Ue, me, 11, this._HE[37]),
            Ue = this._HH(Ue, We, Ne, Fe, Se, 16, this._HE[38]),
            Fe = this._HH(Fe, Ue, We, Ne, ke, 23, this._HE[39]),
            Ne = this._HH(Ne, Fe, Ue, We, Me, 4, this._HE[40]),
            We = this._HH(We, Ne, Fe, Ue, pe, 11, this._HE[41]),
            Ue = this._HH(Ue, We, Ne, Fe, ve, 16, this._HE[42]),
            Fe = this._HH(Fe, Ue, We, Ne, we, 23, this._HE[43]),
            Ne = this._HH(Ne, Fe, Ue, We, Oe, 4, this._HE[44]),
            We = this._HH(We, Ne, Fe, Ue, je, 11, this._HE[45]),
            Ue = this._HH(Ue, We, Ne, Fe, Be, 16, this._HE[46]),
            Ne = this._II(Ne, Fe = this._HH(Fe, Ue, We, Ne, ye, 23, this._HE[47]), Ue, We, pe, 6, this._HE[48]),
            We = this._II(We, Ne, Fe, Ue, Se, 10, this._HE[49]),
            Ue = this._II(Ue, We, Ne, Fe, Re, 15, this._HE[50]),
            Fe = this._II(Fe, Ue, We, Ne, _e, 21, this._HE[51]),
            Ne = this._II(Ne, Fe, Ue, We, je, 6, this._HE[52]),
            We = this._II(We, Ne, Fe, Ue, ve, 10, this._HE[53]),
            Ue = this._II(Ue, We, Ne, Fe, ke, 15, this._HE[54]),
            Fe = this._II(Fe, Ue, We, Ne, ge, 21, this._HE[55]),
            Ne = this._II(Ne, Fe, Ue, We, Ae, 6, this._HE[56]),
            We = this._II(We, Ne, Fe, Ue, Be, 10, this._HE[57]),
            Ue = this._II(Ue, We, Ne, Fe, we, 15, this._HE[58]),
            Fe = this._II(Fe, Ue, We, Ne, Me, 21, this._HE[59]),
            Ne = this._II(Ne, Fe, Ue, We, me, 6, this._HE[60]),
            We = this._II(We, Ne, Fe, Ue, Te, 10, this._HE[61]),
            Ue = this._II(Ue, We, Ne, Fe, ye, 15, this._HE[62]),
            Fe = this._II(Fe, Ue, We, Ne, Oe, 21, this._HE[63]),
            le[0] = le[0] + Ne | 0,
            le[1] = le[1] + Fe | 0,
            le[2] = le[2] + Ue | 0,
            le[3] = le[3] + We | 0
    }

    _signatureFinalizeAfterProcess(hash) {
        for (var he = hash, ge = he.words, ye = 0; ye < 4; ye++) {
            var ve = ge[ye];
            ge[ye] = 16711935 & (ve << 8 | ve >>> 24) | 4278255360 & (ve << 24 | ve >>> 8)
        }
        return he
    }

    _FF(q, ne, oe, ie, se, le, pe) {
        var he = q + (ne & oe | ~ne & ie) + se + pe;
        return (he << le | he >>> 32 - le) + ne
    }

    _GG(q, ne, oe, ie, se, le, pe) {
        var he = q + (ne & ie | oe & ~ie) + se + pe;
        return (he << le | he >>> 32 - le) + ne
    }

    _HH(q, ne, oe, ie, se, le, pe) {
        var he = q + (ne ^ oe ^ ie) + se + pe;
        return (he << le | he >>> 32 - le) + ne
    }

    _II(q, ne, oe, ie, se, le, pe) {
        var he = q + (oe ^ (ne | ~ie)) + se + pe;
        return (he << le | he >>> 32 - le) + ne
    }

}