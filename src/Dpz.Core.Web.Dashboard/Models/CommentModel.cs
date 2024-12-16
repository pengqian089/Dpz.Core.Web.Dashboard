using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dpz.Core.EnumLibrary;

namespace Dpz.Core.Web.Dashboard.Models;

public class CommentModel
{
    public string Id { get; set; }

    /// <summary>
    /// 评论类型
    /// </summary>
    [JsonConverter(typeof(EnumConverter<CommentNode>))]
    public CommentNode Node { get; set; }

    /// <summary>
    /// 关联
    /// </summary>
    public string Relation { get; set; }

    /// <summary>
    /// 回复时间
    /// </summary>
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 回复内容
    /// </summary>
    public string CommentText { get; set; }

    /// <summary>
    /// 回复ID
    /// </summary>
    public List<string> Replies { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 身份标识
    /// </summary>
    public string Identity { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 网站
    /// </summary>
    public string Site { get; set; }

    /// <summary>
    /// 是否匿名评论
    /// </summary>
    public bool IsGuest { get; set; }
    
    /// <summary>
    /// 是否删除
    /// </summary>
    public bool? IsDelete { get; set; }

    /// <summary>
    /// 是否展开完整评论
    /// </summary>
    public bool ShowComment { get; set; } = false;
}