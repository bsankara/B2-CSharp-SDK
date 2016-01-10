using System;
using System.Collections.Generic;

// This is a class for storing all of the different objects that can be returned from b2
// TODO: implement toString() methods for all of these classes for printing debug info more easily
public class B2Bucket
{
    public string accountId { get; set; }
    public string bucketId { get; set; }
    public string bucketName { get; set; }
    public string bucketType { get; set; }
    public B2Bucket(string paramAccountId, string paramBucketId, string paramBucketName, string paramBucketType)
    {
        accountId = paramAccountId;
        bucketId = paramBucketId;
        bucketName = paramBucketName;
        bucketType = paramBucketType;
    }
}

public class B2BucketList
{
    public List<B2Bucket> buckets { get; set; }
}

public class B2FileList
{
    public List<B2File> files { get; set; }
    public string nextFileName { get; set; }

}
public class B2File
{
    public string action { get; set; }
    public string fileId { get; set; }
    public string fileName { get; set; }
    public string size { get; set; }
    public string uploadTimeStamp { get; set; }
}
