/**
 * 播放器配置示例
 * 
 * */
function on_cc_player_init(vid, objectId ){
	var config = {};
	
	//关闭右侧菜单
	config.rightmenu_enable = 0;
	
	config.on_player_seek = "custom_seek";
	config.on_player_ready = "custom_player_ready";
	config.on_player_start = "custom_player_start";
	config.on_player_pause = "custom_player_pause";
	config.on_player_resume = "custom_player_resume";
	config.on_player_stop = "custom_player_stop";
	
	config.player_plugins = {// 插件名称 : 插件参数
			Subtitle : {
				url : "http://dev.bokecc.com/static/font/example.utf8.srt"
				, size : 24
				, color : 0xFFFFFF
				, surroundColor : 0x3c3c3c
				, bottom : 0.15
				, font : "Helvetica"
				, code : "utf-8"
			}
		};
	
	var player = getSWF(objectId);
	player.setConfig(config);
}

function getSWF(objectId) { //获取播放器对象
	if (navigator.appName.indexOf("Microsoft") != -1) {
		return window[objectId];
	} else {
		return document[objectId];
	}
}

/**
 * Player API接口调用示例
 * 
 * */

var prefix = "cc_";
function player_play(id) { // 开始播放
	var player = getSWF(prefix + id);
	player.start();
}

function player_pause(id) { // 暂停播放
	var player = getSWF(prefix + id);
	player.pause();
}

function player_resume(id) { // 恢复播放
	var player = getSWF(prefix + id);
	player.resume();
}

function player_current(id) { // 获取当前播放时间
	var player = getSWF(prefix + id);
	$("#display_info").html("<strong>接口</strong>：getPosition()." 
			+ "&nbsp;<strong>当前播放时间</strong>：<span style='color:#5CB85C;'>" + player.getPosition() + "</span> s.");
}

function player_seek(id) { // 拖动播放
	var player = getSWF(prefix + id);
	player.seek(60);
}

/**
 * 自定义函数示例
 * 
 * */

function custom_player_ready() {
	$("#display_info").html("<strong>播放器配置</strong>：on_player_ready.");

	showBtn("player_start");
	disabledBtn("player_pause");
	disabledBtn("player_resume");
	disabledBtn("player_current");
	disabledBtn("player_seek");
	return;
}

function custom_player_start() {
	$("#display_info").html("<strong>接口</strong>：start().&nbsp;&nbsp;<strong>播放器配置</strong>：on_player_start.");
	
	disabledBtn("player_start");
	showBtn("player_pause");
	showBtn("player_current");
	showBtn("player_seek");
	return;
}

function custom_player_pause() {
	$("#display_info").html("<strong>接口</strong>：pause().&nbsp;&nbsp;<strong>播放器配置</strong>：on_player_pause.");
	
	disabledBtn("player_pause");
	showBtn("player_resume");
	return;
}

function custom_player_resume() {
	$("#display_info").html("<strong>接口</strong>：resume().&nbsp;&nbsp;<strong>播放器配置</strong>：on_player_resume.");
	
	showBtn("player_pause");
	disabledBtn("player_resume");
	return;
}

function custom_seek(from,to){
	$("#display_info").html("拖动播放，从 <span style='color:#5CB85C;'>" + from + "</span> 秒拖动到第 <span style='color:#5CB85C;'>" + to + "</span> 秒");
}

function custom_player_stop() {
	$("#display_info").html("<strong>播放器配置</strong>：on_player_stop.");
	
	disabledBtn("player_pause");
	showBtn("player_resume");
	return;
}

$(function() {
	disabledBtn = function(name){
		var playerBtn = $("#" + name);
		if(!playerBtn.prop("disabled")){
				playerBtn.attr("disabled","disabled");
		}
	};
	
	showBtn = function(name){
		var playerBtn = $("#" + name);
		if(playerBtn.prop("disabled")){
			playerBtn.removeAttr("disabled");
		} 
	};
});
