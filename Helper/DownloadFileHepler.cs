
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGS.Helper;
using System.Net;
using System.IO;
using System.Windows;

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
                                  bool bOnlyGetSize                   = false)
        {
            UpdateDownloadNotify UpdateMothed     = UpdateFunc == null ? null : new UpdateDownloadNotify(UpdateFunc);
            CompleteDownloadNotify CompleteMothed = CompleteFunc == null ? null : new CompleteDownloadNotify(CompleteFunc);
            ErrDownloadNotify ErrMothed           = ErrFunc == null ? null : new ErrDownloadNotify(ErrFunc);
            long lAlreadyDownloadSize             = 0;
            long lTotalSize                       = 0;
            long lIncreSize                       = 0;

            if (RetryNum > 50)
                RetryNum = 50;

            RETRY_ENTRY:
            try
            {
                ServicePointManager.Expect100Continue = false;

                HttpWebRequest request    = (HttpWebRequest)WebRequest.Create(sUrl);
                request.Method            = "GET";
                request.ContentType       = ContentType;
                request.Timeout           = Timeout;
                //request.KeepAlive       = true;
                request.UserAgent         = UserAgent;

                //开始请求
                HttpWebResponse response  = (HttpWebResponse)request.GetResponse();
                lTotalSize = response.ContentLength;
                if (bOnlyGetSize)
                    return lTotalSize;

                //创建目录
                string pDir = Path.GetDirectoryName(sPath);
                PathHelper.Mkdirs(pDir);

                //打开文件
                Stream myResponseStream   = response.GetResponseStream();
                System.IO.Stream pFD      = new System.IO.FileStream(sPath, System.IO.FileMode.Create);

                //如果走到这里的话，就不能重试了，要不如进度会出错
                RetryNum = 0;

                byte[] buf = new byte[1024];
                int size   = 0;
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
                
                if (CompleteMothed != null)
                {
                    CompleteMothed(lTotalSize, data);
                }

            RETURN_POINT:
                pFD.Close();
                myResponseStream.Close();
                return true;
            }
            catch(System.Exception e)
            {
                if(RetryNum > 0)
                {
                    RetryNum--;
                    goto RETRY_ENTRY;
                }

                if (ErrMothed != null)
                {
                    ErrMothed(lTotalSize, lAlreadyDownloadSize, e.Message, data);
                }
                if (bOnlyGetSize)
                    return 0;
                return false;
            }
        }
    }



}
