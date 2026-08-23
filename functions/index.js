const functions = require("firebase-functions/v1");
const admin = require("firebase-admin");
const cors = require("cors")({ origin: true });

admin.initializeApp();
const db = admin.firestore();

// Authorization: Bearer {idToken} 헤더를 검증하고 uid를 반환한다.
// 실패 시 직접 res에 에러 응답을 쓰고 null을 반환한다 (호출부는 null이면 바로 return).
async function verifyAuth(req, res) {
  const authHeader = req.get("Authorization") || "";
  const match = authHeader.match(/^Bearer (.+)$/);
  if (!match) {
    res.status(401).json({ error: "missing_token" });
    return null;
  }
  try {
    const decoded = await admin.auth().verifyIdToken(match[1]);
    return decoded.uid;
  } catch (err) {
    res.status(401).json({ error: "invalid_token" });
    return null;
  }
}

// 내 보드 스냅샷을 업로드한다. 같은 라운드에 이미 있던 내 문서는 덮어쓴다(누적 아님).
exports.uploadSnapshot = functions.https.onRequest((req, res) => {
  cors(req, res, async () => {
    if (req.method !== "POST") {
      res.status(405).json({ error: "method_not_allowed" });
      return;
    }

    const uid = await verifyAuth(req, res);
    if (!uid) return;

    const { waveIndex, boardJson } = req.body || {};
    if (typeof waveIndex !== "number" || typeof boardJson !== "string") {
      res.status(400).json({ error: "invalid_body" });
      return;
    }

    await db
      .collection("asyncPvpSnapshots")
      .doc(String(waveIndex))
      .collection("entries")
      .doc(uid)
      .set({
        ownerId: uid,
        waveIndex,
        boardJson,
        updatedAt: admin.firestore.FieldValue.serverTimestamp(),
      });

    res.status(200).json({ success: true });
  });
});

// 해당 라운드에 저장된 스냅샷 중 내 것을 제외하고 무작위로 하나 반환한다.
exports.getOpponentSnapshot = functions.https.onRequest((req, res) => {
  cors(req, res, async () => {
    if (req.method !== "POST") {
      res.status(405).json({ error: "method_not_allowed" });
      return;
    }

    const uid = await verifyAuth(req, res);
    if (!uid) return;

    const { waveIndex } = req.body || {};
    if (typeof waveIndex !== "number") {
      res.status(400).json({ error: "invalid_body" });
      return;
    }

    const snapshot = await db
      .collection("asyncPvpSnapshots")
      .doc(String(waveIndex))
      .collection("entries")
      .get();

    const candidates = snapshot.docs.filter((doc) => doc.id !== uid);
    if (candidates.length === 0) {
      res.status(200).json({ found: false });
      return;
    }

    const chosen = candidates[Math.floor(Math.random() * candidates.length)];
    const data = chosen.data();
    res.status(200).json({
      found: true,
      ownerId: data.ownerId,
      boardJson: data.boardJson,
    });
  });
});
