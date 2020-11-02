using AIGS.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AIGS.Helper
{
    public class DownloadFileHepler
    {
        /// <summary>
        /// 获取文件大小
        /// </summary>
        public static long GetFileLength(string url)
        {
            try
            {
                return (long)Start(url, null, bOnlyGetSize: true, Timeout:3000, RetryNum:3, UserAgent:null, ContentType:null);
            }
            catch
            {
                 
            }
            return 0;
        }

        /// <summary>
        /// 获取全部文件的大小
        /// </summary>
        public static long GetAllFileLength(string[] urls)
        {
            long lLength = 0;
            foreach (string item in urls)
            {
                lLength += GetFileLength(item);
            }
            return lLength;
        }

        /// <summary>
        /// 进度更新响应
        /// </summary>
        /// <param name="lTotalSize">文件大小</param>
        /// <param name="lAlreadyDownloadSize">已下载的大小</param>
        /// <param name="lIncreSize">增量下载的大小</param>
        /// <param name="data">中间传递的数据</param>
        /// <returns>False: Stop Download</returns>
        public delegate bool UpdateDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, long lIncreSize, object data);
        /// <summary>
        /// 完成下载
        /// </summary>
        /// <param name="lTotalSize">文件大小</param>
        /// <param name="data">中间传递的数据</param>
        public delegate void CompleteDownloadNotify(long lTotalSize, object data);
        /// <summary>
        /// 错误情况
        /// </summary>
        /// <param name="lTotalSize">文件大小</param>
        /// <param name="lAlreadyDownloadSize">已下载的大小</param>
        /// <param name="sErrMsg">错误信息</param>
        /// <param name="data">中间传递的数据</param>
        public delegate void ErrDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, string sErrMsg, object data);

        /// <summary>
        /// 启动下载
        /// </summary>
        /// <param name="sUrl">下载链接</param>
        /// <param name="sPath">路径文件名</param>
        /// <param name="data">中间传递的数据</param>
        /// <param name="UpdateFunc">进度更新</param>
        /// <param name="CompleteFunc">下载结束</param>
        /// <param name="ErrFunc">错误</param>
        /// <param name="Timeout">超时</param>
        /// <param name="UserAgent"></param>
        /// <param name="ContentType"></param>
        public static object Start(string sUrl,
                                  string sPath,
                                  object data                         = null,
                                  UpdateDownloadNotify UpdateFunc     = null,
                                  CompleteDownloadNotify CompleteFunc = null,
                                  ErrDownloadNotify ErrFunc           = null,
                                  int RetryNum                        = 0,
                                  int Timeout                         = 5 * 1000,
                                  string UserAgent                    = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                  string ContentType                  = "application/x-www-form-urlencoded; charset=UTF-8",
                                  bool bOnlyGetSize                   = false,
                                  bool bAppendFile                    = false,
                                  HttpHelper.ProxyInfo Proxy          = null,
                                  int iRangeFrom                      = -1,
                                  int iRangeTo                        = -1)
        {
            var oBj = StartAsync(sUrl, sPath, data, UpdateFunc, CompleteFunc, ErrFunc, RetryNum, Timeout, UserAgent, ContentType, bOnlyGetSize, bAppendFile, Proxy, iRangeFrom, iRangeTo);
            return oBj.Result;
        }

        /// <summary>
        /// 启动下载
        /// </summary>
        /// <param name="sUrl">下载链接</param>
        /// <param name="sPath">路径文件名</param>
        /// <param name="data">中间传递的数据</param>
        /// <param name="UpdateFunc">进度更新</param>
        /// <param name="CompleteFunc">下载结束</param>
        /// <param name="ErrFunc">错误</param>
        /// <param name="Timeout">超时</param>
        /// <param name="UserAgent"></param>
        /// <param name="ContentType"></param>
        public static Task<object> StartAsync(string sUrl,
                                  string sPath,
                                  object data                         = null,
                                  UpdateDownloadNotify UpdateFunc     = null,
                                  CompleteDownloadNotify CompleteFunc = null,
                                  ErrDownloadNotify ErrFunc           = null,
                                  int RetryNum                        = 0,
                                  int Timeout                         = 5 * 1000,
                                  string UserAgent                    = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                  string ContentType                  = "application/x-www-form-urlencoded; charset=UTF-8",
                                  bool bOnlyGetSize                   = false,
                                  bool bAppendFile                    = false,
                                  HttpHelper.ProxyInfo Proxy          = null,
                                  int iRangeFrom                      = -1,
                                  int iRangeTo                        = -1)
        {
            return Task.Run(() =>
            {
                UpdateDownloadNotify UpdateMothed = UpdateFunc == null ? null : new UpdateDownloadNotify(UpdateFunc);
                CompleteDownloadNotify CompleteMothed = CompleteFunc == null ? null : new CompleteDownloadNotify(CompleteFunc);
                ErrDownloadNotify ErrMothed = ErrFunc == null ? null : new ErrDownloadNotify(ErrFunc);
                long lAlreadyDownloadSize = 0;
                long lTotalSize = 0;
                long lIncreSize = 0;
                bool bAddRange = false;
                System.IO.Stream pFD = null;

                if (RetryNum > 50)
                    RetryNum = 50;

                RETRY_ENTRY:
                try
                {
                    bool bRet = false;
                    ServicePointManager.Expect100Continue = false;

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
                    request.Method = "GET";
                    request.ContentType = ContentType;
                    request.Timeout = Timeout;
                    //request.KeepAlive       = true;
                    request.UserAgent = UserAgent;
                    request.Proxy = null;

                    if (iRangeFrom != -1 && iRangeTo != -1)
                    {
                        bAddRange = true;
                        request.AddRange(iRangeFrom, iRangeTo);
                    }

                    if (Proxy != null && Proxy.Host.IsNotBlank() && Proxy.Port >= 0)
                    {
                        WebProxy myProxy = new WebProxy(Proxy.Host, Proxy.Port);
                        if (Proxy.Username.IsNotBlank() && Proxy.Password.IsNotBlank())
                            myProxy.Credentials = new NetworkCredential(Proxy.Username, Proxy.Password);

                        request.Proxy = myProxy;
                        request.Credentials = CredentialCache.DefaultNetworkCredentials;
                    }

                    //开始请求
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    lTotalSize = response.ContentLength;
                    if (bAddRange)
                        lTotalSize = iRangeTo - iRangeFrom;
                    if (bOnlyGetSize)
                        return (object)lTotalSize;

                    //创建目录
                    string pDir = Path.GetDirectoryName(sPath);
                    PathHelper.Mkdirs(pDir);

                    if(File.Exists(sPath) && !bAppendFile)
                        File.Delete(sPath);

                    //打开文件
                    Stream myResponseStream = response.GetResponseStream();
                    //一分钟超时
                    myResponseStream.ReadTimeout = 60000;
                    pFD = new System.IO.FileStream(sPath, bAppendFile ? System.IO.FileMode.Append : System.IO.FileMode.Create);

                    //如果走到这里的话，就不能重试了，要不如进度会出错
                    RetryNum = 0;
                    byte[] buf = new byte[1024];
                    int size = 0;
                    while ((size = myResponseStream.Read(buf, 0, (int)buf.Length)) > 0)
                    {
                        lIncreSize = size;
                        lAlreadyDownloadSize += size;
                        pFD.Write(buf, 0, size);
                        if (UpdateFunc != null)
                        {
                            if (!UpdateMothed(lTotalSize, lAlreadyDownloadSize, lIncreSize, data))
                                goto RETURN_POINT;
                        }
                    }
                    bRet = true;

                RETURN_POINT:
                    if(pFD != null)
                    {    
                        pFD.Close();
                        File.Delete(sPath);
                    }
                    if(myResponseStream != null)
                        myResponseStream.Close();
                    if(bRet && CompleteMothed != null)
                        CompleteMothed(lTotalSize, data);
                    return bRet;
                }
                catch (System.Exception e)
                {
                    if (pFD != null)
                    {
                        pFD.Close();
                        File.Delete(sPath);
                    }
                    if (RetryNum > 0)
                    {
                        RetryNum--;
                        goto RETRY_ENTRY;
                    }

                    if (ErrMothed != null)
                    {
                        ErrMothed(lTotalSize, lAlreadyDownloadSize, e.Message, data);
                    }
                    if (bOnlyGetSize)
                        return (object)0;
                    return (object)false;
                }
            });
        }



        


        //public static Task<bool> StartMultiThreadAsync(string sUrl,
        //                  string sPath,
        //                  object data = null,
        //                  UpdateDownloadNotify UpdateFunc = null,
        //                  CompleteDownloadNotify CompleteFunc = null,
        //                  ErrDownloadNotify ErrFunc = null,
        //                  int RetryNum = 0,
        //                  int Timeout = 5 * 1000,
        //                  string UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
        //                  string ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
        //                  HttpHelper.ProxyInfo Proxy = null,
        //                  int ThreadNum = 30,
        //                  int PartSize = 1048576)
        //{
        //    return Task.Run(() =>
        //    {
        //        object iFileLength = StartAsync(sUrl, null, UserAgent: UserAgent, ContentType: ContentType, Proxy: Proxy).Result;
        //        if (iFileLength == null || int.Parse(iFileLength.ToString()) == 0)
        //            return false;

        //        List<int[]> lRanges = new List<int[]>();
        //        int iAt = 0;
        //        int iLength = int.Parse(iFileLength.ToString());
        //        while(iLength > 0)
        //        {
        //            if (iLength > PartSize)
        //                lRanges.Add(new int[2] { iAt, iAt + PartSize });
        //            else
        //                lRanges.Add(new int[2] { iAt, iAt + iLength });
        //            iAt += PartSize;
        //            iLength -= PartSize;
        //        }

        //        //创建目录
        //        string sTmpPath = Path.GetDirectoryName(sPath);
        //        var di = new DirectoryInfo(sTmpPath);
        //        if (!di.Exists)
        //            di.Create();

        //        ThreadPoolManager Pool = new ThreadPoolManager(ThreadNum, true);
        //        List<string> lFiles = new List<string>();
        //        for (int i = 0; i < lRanges.Count; i++)
        //        {
        //            string sFilePath = sTmpPath + '/' + i.ToString() + ".part";
        //            lFiles.Add(sFilePath);
        //            //Pool.AddWork()
        //        }

        //        Pool.WaitAll();

        //        return true;
        //        //return bRet;
        //    });
        //}
    }



}
