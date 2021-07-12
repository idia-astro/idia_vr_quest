import * as fs from "fs/promises";

export async function getMagicNumber(fd: fs.FileHandle) {
    const bytes = new Uint8Array(8);
    const readResult = await fd.read(bytes, 0, 8, 0);
    if (readResult.bytesRead === 8) {
        const dv = new DataView(bytes.buffer);
        return dv.getBigUint64(0);
    } else {
        return null;
    }
}