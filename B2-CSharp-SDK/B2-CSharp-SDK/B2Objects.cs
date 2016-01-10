using System;
using System.Collections.Generic;

// This is a class for storing all of the different objects that can be returned from b2
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