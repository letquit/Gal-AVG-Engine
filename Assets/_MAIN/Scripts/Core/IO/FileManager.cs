using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// 文件管理器类，提供文件读写和安全的存档加密功能。
/// 使用 AES-256 加密和 HMAC-SHA256 签名来保护存档数据，防止篡改。
/// </summary>
public class FileManager
{
    // --- AES + HMAC 安全密钥 ---
    // 重要提示：在实际发布版本中不应硬编码这些密钥。
    // 可以考虑使用代码混淆、从服务器获取或分段存储等方式来保护它们。
    // AES 密钥长度必须是 16, 24, 或 32 字节 (对应 AES-128, AES-192, AES-256)。
    private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("iXvDD0vkZo1NsVQYePytoOUjWCn99qGL");
    private static readonly byte[] HmacKey = Encoding.UTF8.GetBytes("KHsXvgfolfFCnhGFih2RFISJsIBQS81m");

    /// <summary>
    /// 将JSON数据保存到指定文件路径。
    /// </summary>
    /// <param name="filePath">要保存的文件路径</param>
    /// <param name="JSONData">要保存的JSON数据字符串</param>
    /// <param name="encrypt">是否对数据进行加密保存，默认为true</param>
    public static void Save(string filePath, string JSONData, bool encrypt = true)
    {
        if (!TryCreateDirectoryFromPath(filePath))
        {
            Debug.LogError($"[FileManager] 无法创建目录，保存失败: '{filePath}'");
            return;
        }

        if (encrypt)
        {
            try
            {
                // 1. 将字符串数据转为字节数组
                byte[] plainBytes = Encoding.UTF8.GetBytes(JSONData);

                // 2. 使用AES加密数据
                byte[] encryptedBytes;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.GenerateIV(); // 为每次加密生成一个唯一的初始化向量 (IV)
                    byte[] iv = aes.IV;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        // 将IV写入流的前面，解密时需要它
                        ms.Write(iv, 0, iv.Length);

                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        encryptedBytes = ms.ToArray(); // 此时包含 [IV][EncryptedData]
                    }
                }

                // 3. 为加密后的数据（包含IV）计算HMAC签名
                byte[] hmac;
                using (HMACSHA256 hmacSha256 = new HMACSHA256(HmacKey))
                {
                    hmac = hmacSha256.ComputeHash(encryptedBytes);
                }

                // 4. 将HMAC签名和加密数据写入文件
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    // 文件结构: [HMAC Signature][IV][EncryptedData]
                    fs.Write(hmac, 0, hmac.Length);
                    fs.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileManager] 加密并保存文件时出错: {e}");
                return;
            }
        }
        else
        {
            // 非加密方式保存
            File.WriteAllText(filePath, JSONData, Encoding.UTF8);
        }
        
        Debug.Log($"[FileManager] 数据成功保存到 '{filePath}'");
    }

    /// <summary>
    /// 从指定文件路径加载并反序列化JSON数据。
    /// </summary>
    /// <typeparam name="T">要反序列化的目标类型</typeparam>
    /// <param name="filePath">要加载的文件路径</param>
    /// <param name="encrypt">文件是否被加密，默认为false</param>
    /// <returns>反序列化后的对象，如果文件不存在、被篡改或解密失败，则返回default(T)</returns>
    public static T Load<T>(string filePath, bool encrypt = false)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[FileManager] 文件不存在: '{filePath}'");
            return default(T);
        }

        string jsonString = null;

        if (encrypt)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);

                // 1. 分离HMAC和加密数据
                const int hmacSize = 32; // HMAC-SHA256 生成32字节哈希
                if (fileBytes.Length <= hmacSize)
                {
                    Debug.LogError("[FileManager] 存档文件已损坏或被篡改 (文件大小无效)。");
                    return default(T);
                }

                byte[] receivedHmac = new byte[hmacSize];
                byte[] encryptedDataWithIv = new byte[fileBytes.Length - hmacSize];

                Buffer.BlockCopy(fileBytes, 0, receivedHmac, 0, hmacSize);
                Buffer.BlockCopy(fileBytes, hmacSize, encryptedDataWithIv, 0, encryptedDataWithIv.Length);

                // 2. 验证HMAC签名
                using (HMACSHA256 hmacSha256 = new HMACSHA256(HmacKey))
                {
                    byte[] calculatedHmac = hmacSha256.ComputeHash(encryptedDataWithIv);
                    if (!CompareByteArrays(receivedHmac, calculatedHmac))
                    {
                        Debug.LogError("[FileManager] 存档文件已被篡改！HMAC验证失败。");
                        return default(T);
                    }
                }
                
                // 3. HMAC验证通过，使用AES解密数据
                byte[] decryptedBytes;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    
                    // 从加密数据中提取IV
                    int ivSize = aes.BlockSize / 8;
                    if (encryptedDataWithIv.Length < ivSize)
                    {
                        Debug.LogError("[FileManager] 存档文件已损坏 (缺少IV)。");
                        return default(T);
                    }

                    byte[] iv = new byte[ivSize];
                    Buffer.BlockCopy(encryptedDataWithIv, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            // 只解密IV之后的数据部分
                            cs.Write(encryptedDataWithIv, iv.Length, encryptedDataWithIv.Length - iv.Length);
                            cs.FlushFinalBlock();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }
                
                // 4. 将解密后的字节转为字符串
                jsonString = Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileManager] 加载或解密文件时出错: {e}");
                return default(T);
            }
        }
        else
        {
            // 非加密方式加载
            jsonString = File.ReadAllText(filePath, Encoding.UTF8);
        }

        // 5. 反序列化JSON
        if (jsonString != null)
        {
            return JsonUtility.FromJson<T>(jsonString);
        }

        return default(T);
    }
    
    /// <summary>
    /// 读取指定路径的文本文件
    /// </summary>
    /// <param name="filePath">要读取的文件路径</param>
    /// <param name="includeBlankLines">是否包括空行，默认为true</param>
    /// <returns>按行分割的内容列表</returns>
    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        if (!filePath.StartsWith('/'))
            filePath = FilePaths.root + filePath;
        
        List<string> lines = new List<string>();

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError($"File not found: '{ex.FileName}'");
        }
        
        return lines;
    }
    
    /// <summary>
    /// 从Resources文件夹中读取文本资源
    /// </summary>
    /// <param name="filePath">资源路径（相对于Resources文件夹）</param>
    /// <param name="includeBlankLines">是否包括空行，默认为true</param>
    /// <returns>按行分割的内容列表</returns>
    public static List<string> ReadTextAsset(string filePath, bool includeBlankLines = true)
    {
        TextAsset asset = Resources.Load<TextAsset>(filePath);
        if (asset == null)
        {
            Debug.LogError($"Asset not found: '{filePath}'");
            return null;
        }
        
        return ReadTextAsset(asset, includeBlankLines);
    }

    /// <summary>
    /// 读取TextAsset对象
    /// </summary>
    /// <param name="asset">要读取的TextAsset对象</param>
    /// <param name="includeBlankLines">是否包括空行，默认为true</param>
    /// <returns>按行分割的内容列表</returns>
    public static List<string> ReadTextAsset(TextAsset asset, bool includeBlankLines = true)
    {
        List<string> lines = new List<string>();
        using (StringReader sr = new StringReader(asset.text))
        {
            while (sr.Peek() > -1)
            {
                string line = sr.ReadLine();
                if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }
        }
        
        return lines;
    }

    /// <summary>
    /// 尝试从给定路径创建目录
    /// </summary>
    /// <param name="path">目标路径</param>
    /// <returns>如果成功创建或目录已存在则返回true，否则返回false</returns>
    public static bool TryCreateDirectoryFromPath(string path)
    {
        if (Directory.Exists(path) || File.Exists(path))
            return true;

        if (path.Contains("."))
        {
            path = Path.GetDirectoryName(path);
            if (Directory.Exists(path))
                return true;
        }
        
        if (path == string.Empty)
            return false;

        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not create directory! {e}");
            return false;
        }
    }
    
    /// <summary>
    /// 使用固定时间比较字节数组，以防止时序攻击。
    /// </summary>
    /// <param name="a">第一个字节数组</param>
    /// <param name="b">第二个字节数组</param>
    /// <returns>若两个数组完全相同则返回true，否则返回false</returns>
    private static bool CompareByteArrays(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }
        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }
}