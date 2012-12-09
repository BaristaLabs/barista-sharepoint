/**
 * Author: Max Chirkov
 * Email: max.chirkov@gmail.com
 * Website: www.SimpleRealtyTheme.com
 *
 * Version: 2.5
 */
var pullouts_container = false;
var pow_widgets = new Array;
var pow_opened = '';
var pow_mouse_is_inside = false;
var trigger_on = false;
var trigger_lunched = false;
var processing = new Array;

if(typeof powVars != 'undefined'){

    //hiding widgets that should be pullouts from loading on the screen
    //otherwise they will show on the screen and then relocate to their pullout positions
    //in a split second.. kinda lame :)
    for(i in powVars){
        if( document.getElementById(i) ){
            document.getElementById(i).style.display = 'none';
        }else{
            delete powVars[i];
        }
    }

    //important to do on window load, otherwise can't get the true height of some elements
    jQuery(window).load(function(){
        setTimeout('pullouts();', 100);
        //defining pullouts container
        pullouts_container = jQuery('<div id="pullouts" />');
        //append pullouts to the bottom of the body
        jQuery('body').append(pullouts_container);
    });

    function pullouts(){
    //jQuery(document).ready(function(){
        var opposits = { 'top' : 'bottom', 'bottom' : 'top', 'left' : 'right', 'right' : 'left' };
        var IE = false;
        if ( jQuery.browser.msie ){
            var ie_v = parseInt(jQuery.browser.version, 10);
            if( ie_v < 9 )
                IE = true;
        }

        var n = 0;
        for(i in powVars){
            n++;

            //detach widget from the sidebar
            var widget = jQuery('#' + i );
            //make it visible again
            widget.css({'display':'block'});
            widget.css('width', 'auto'); //reset widget width to auto

            //create a pullout object
            var pullout = jQuery('<div id="pullout-' + n + '" class="pullouts"><div class="pullout-content clearfix">');
            //append widget to the pullout object
            pullout.find('.pullout-content').append(widget);
            pullout.appendTo(pullouts_container);

            var widget_width = false;
            var pullout_width = false;
            var pullout_height = false;
            var position = false;
            var label_open = label_close = '';


            if( !powVars[i]['style']['no_label'] || !powVars[i]['style']['icon'] ){
                //default tab labels
                if(powVars[i]['style']['open_label']){
                    label_open = powVars[i]['style']['open_label'];
                }else
                if(powVars[i]['style']['label']){
                    label_open = powVars[i]['style']['label'];
                }else{
                    label_open = 'Open';
                }

                if(powVars[i]['style']['close_label']){
                    label_close = powVars[i]['style']['close_label']
                }else
                if(powVars[i]['style']['open_label']){
                    label_close = powVars[i]['style']['open_label'];
                }else
                if(powVars[i]['style']['label']){
                    label_close = powVars[i]['style']['label'];
                }else{
                    label_close = 'Close';
                }
            }

            //first set pullout width, because content flow depends on it so as pullout's height
            //if the width was defined explicitly
            if(powVars[i]['style']['width']){
                pullout_width = parseInt(powVars[i]['style']['width']);
            }

            if(!pullout_width){
                pullout_width = pullout.outerWidth(true);
            }
            //explicitly set pullout width
            pullout.css('width', pullout_width+'px');

            //get side
            var side = powVars[i]['position']['side'];
            pullout.addClass('side_' + side);

            //set styles if they were selected
            if( powVars[i]['style']['rounded'] ){
                pullout.addClass('rounded');
            }
            if( powVars[i]['style']['borders'] ){
                pullout.addClass('borders');
            }

            var tab_offset = 0; // in percentile
            if( powVars[i]['style']['tab_offset'] )
                tab_offset = powVars[i]['style']['tab_offset'];

            //set scrollable widgets except the bottom one
            if( powVars[i]['position']['scroll'] && 'bottom' != side ){
                pullout.css( 'position', 'absolute');
            }

            //define anchor
            if( powVars[i]['position']['anchor'] == '0' ){
                var v_anchor = 'top';
                var h_anchor = 'left';
            }else{
                var v_anchor = 'bottom';
                var h_anchor = 'right';
            }

            //add open/close tab
            var pullout_button = jQuery('<div class="pullout-button"><span>'+ label_open + '</span></div>');
            pullout.append(pullout_button);

            //check if tab icon set
            if( powVars[i]['style']['icon'] ){
                var icon = powVars[i]['style']['icon'];
                var coord = icon.split('_');
                var grid = 36;
                var x = grid * parseInt(coord[0]);
                var y = grid * parseInt(coord[1]);
                pullout_button.prepend('<div class="icon ' + icon + '"></div>');

                pullout.find('.icon.'+icon).css(
                    'background-position', '-' + x + 'px' + ' -' + y + 'px'
                );
            }
            if(label_open.length < 1){
                pullout.find('.pullout-button span').remove();
                pullout.find('.icon').css('margin', '10px');
            }


            //after adding borders class, we need to recalculate the width,
            //unfortunately outerWidth executes too soon and doesn't include the width of the border
            //so we calculate it separately
            var border_width = {
                'top': 0,
                'right': 0,
                'bottom': 0,
                'left': 0
            };
            var _w = parseInt( pullout_button.css('border-left-width') );
            border_width.left = ( isNaN( _w ) ) ? 0 : _w;
            var _w = parseInt( pullout_button.css('border-right-width') );
            border_width.right = ( isNaN( _w ) ) ? 0 : _w;
            var _w = parseInt( pullout_button.css('border-top-width') );
            border_width.top =  ( isNaN( _w ) ) ? 0 : _w;
            var _w = parseInt( pullout_button.css('border-bottom-width') );
            border_width.bottom =  ( isNaN( _w ) ) ? 0 : _w;

            var pullout_button_width = pullout_button.outerWidth(false);
            var pullout_button_height = pullout_button.outerHeight(false);
            //Some themes like Gesis and Twenty Eleven need width of the borders to be added
            //to the width of the tab.
            //We need to get only 2 borders, but don't know which ones defined,
            //because tabs could be vertical and borders switch sides.
            //We know that in any case only 3 borders will be defined.
            var _two_borders_width = (border_width.left + border_width.right + border_width.top + border_width.bottom) / 3 * 2;

            pullout_button.css('min-width', pullout_button_width + 'px');

            //get and set the height after the width has been set so we aquire the correct height
            //pullout.css('padding', '0px');
            pullout_height = pullout.outerHeight(false);

            //define position based on the side
            //according to width/height the pullouts will slide within the same dimensions
            if( side == 'left' || side == 'right' ){
                //set min-height on the side so it could expand if needed
                pullout.css('min-height', pullout_height+'px');
                position = pullout_width + border_width[opposits[side]];
               // position = 260;

                var btn_dimensions = pullout_button_width + parseInt(border_width[opposits[side]]);
                //calculating button offset based on tab_offset parameter
                btn_offset = get_tab_offset( pullout_height + (border_width.top * 2) - pullout_button_height );
                //console.log('pullout_height: ' + pullout_height + ' _two_borders_width: ' + _two_borders_width + ' pullout_button_height: ' + pullout_button_height + ' tab_offset: ' + tab_offset);
                var tab_position = (border_width.top * -1) + btn_offset;
                //specify positioning of the pullout button
                pullout_button.css(opposits[side], '-' + btn_dimensions + 'px')
                .css('top', tab_position  + 'px');
            }else{
                //set static height on top and buttom since min-height will affect the hiding effect
                //and the content will be sticking out more than needed when in hidden/closed state
                pullout.css('height', pullout_height+'px');

                position = pullout_height + parseInt(border_width[opposits[side]]);
                var btn_dimensions = pullout_button_height;
                btn_offset = get_tab_offset( pullout_width - pullout_button_width );

                var tab_position = (border_width.left * -1) + btn_offset;
                //specify positioning of the pullout button
                pullout_button.css(opposits[side], '-' + btn_dimensions + 'px')
                .css('left', tab_position + 'px');
            }

            if( !IE && powVars[i]['style']['rotate'] && ('left' == side || 'right' == side) ){
                pullout_button.addClass('rotate');

                var btn_dimensions = pullout_button_height - parseInt(border_width[opposits[side]]);
                btn_offset = get_tab_offset(pullout_height - pullout_button_width );
                var tab_position = pullout_button_height*-1 + btn_offset;

                pullout_button.css(opposits[side], '-' + btn_dimensions + 'px')
                .css('top', tab_position + 'px');
            }

            //position anchor via css
            if( side == 'left' || side == 'right' ){
                pullout.css( v_anchor, powVars[i]['position']['distance'] );
            }else{
                pullout.css( h_anchor, powVars[i]['position']['distance'] );
            }

            //re-position widget on selected side - hide it and add class closed
            pullout.css(side, '-' + position + 'px').addClass('pullout-closed');

            //set color

            if(powVars[i]['style']['color']){
                pullout.css('background-color', powVars[i]['style']['color']);
                pullout_button.css('background-color', powVars[i]['style']['color']);
            }
            if(powVars[i]['style']['text_color'])
                pullout.css('color', powVars[i]['style']['text_color']);

            if(powVars[i]['style']['link_color'])
                pullout.find('a').css('color', powVars[i]['style']['link_color']);

            if(powVars[i]['style']['borders'] && powVars[i]['style']['border_color'])
                pullout.css('border-color', powVars[i]['style']['border_color']);
                pullout_button.css('border-color', powVars[i]['style']['border_color']);


            pow_widgets.push( {
                id: 'pullout-' + n,
                position: position,
                side: side,
                label_open: label_open,
                label_close: label_close,
                trigger_on: powVars[i]['style']['show_on'],
                speed: powVars[i]['style']['speed'],
                timer: parseInt(powVars[i]['behavior']['timer'])*1000,
                element: powVars[i]['behavior']['element'],
                easing: powVars[i]['behavior']['easing'],
                n_pages: powVars[i]['behavior']['n_pages']
            } );

        }

        //setup listeners
        pow_listen_appear();
        pow_start_page_count();
        pow_start_timer();

        //mouse location listener
        jQuery('.pullouts').mouseenter(function(){
            pow_mouse_is_inside = true;

            //reset trigger bindings
            pow_reset_trigger();
            //get current id
            var current_id = jQuery(this).attr('id');
            var widget = get_pow(current_id);
            var trigger_on = widget['trigger_on'];

            //bind respective behaviours
            if( 'mouseover' == trigger_on ){
                pow_mouseover(widget['id']);
            }else{
                pow_click(widget['id']);
            }


        }).mouseleave(function(){
            pow_mouse_is_inside = false;
        });


        //if click outside of pullout area - close pullout
        jQuery('body').mouseup(function(){
            if( !pow_mouse_is_inside ){
                pow_close(pow_opened);
            }
        });

        function get_tab_offset(side_size){
            var tab_offset = (powVars[i]['style']['tab_offset']) ? parseInt(powVars[i]['style']['tab_offset']) : 0;
            switch(powVars[i]['style']['tab_offset_type']){
                case '%':
                    var offset = Math.round(side_size*tab_offset/100);
                    //round straight corners
                    round_straight_corners(powVars[i]['position']['side'], offset, side_size);
                    return offset;
                case 'px':
                    var offset = tab_offset;
                    round_straight_corners(powVars[i]['position']['side'], offset, side_size);
                    return offset;
            }
        }

        function round_straight_corners(side, offset, side_size){
            if(!powVars[i]['style']['rounded'])
                return false;
            if(offset > 0 && offset < side_size){
                if( 'left' == side || 'right' == side ){
                    pullout.css("border-bottom-" + opposits[side] + "-radius", "10px");
                    pullout.css("border-top-" + opposits[side] + "-radius", "10px");
                }
                if( 'top' == side || 'bottom' == side ){
                    pullout.css("border-" + opposits[side] + "-left-radius", "10px");
                    pullout.css("border-" + opposits[side] + "-right-radius", "10px");
                }
            }else
            if( offset == side_size ){

                if( 'left' == side || 'right' == side ){
                    pullout.css("border-bottom-" + opposits[side] + "-radius", "0px");
                    pullout.css("border-top-" + opposits[side] + "-radius", "10px");
                }
                if( 'top' == side || 'bottom' == side ){
                    pullout.css("border-" + opposits[side] + "-left-radius", "10px");
                    pullout.css("border-" + opposits[side] + "-right-radius", "0px");
                }
            }
        }


    }//);

}

function pow_listen_appear(){
    for(i in pow_widgets){
        var id = pow_widgets[i].id;
        if( ('appear' == pow_widgets[i].trigger_on || 'appear_once' == pow_widgets[i].trigger_on) && pow_widgets[i].element.length > 0 ){
            var once = false;

            if('appear_once' == pow_widgets[i].trigger_on)
                once = true;

            jQuery(pow_widgets[i].element).appear(function(event, id){
                do_pullout(id);
            },{
                one: once,
                data: [id]
            });
        }
    }
}


function pow_click(id){
    jQuery('.pullout-button').click(function(){
       do_pullout(id);
    });

}

function pow_mouseover(id){
    //preventing slide on/off on tab hover
    if( pow_opened != id ){
        do_pullout(id);
    }

    //if mouse moves outside of pullout area - close pullout
    jQuery('#'+id).bind('mouseleave', function(){
       var t = setTimeout("pow_delay_mouseout_close()", 500);
    });
}


//when widget closes on mouseleave, we set delay so it doesn't
//go all crazy on multiple mouse-in/out.
function pow_delay_mouseout_close(){
    if(!pow_mouse_is_inside)
        pow_close(pow_opened);
}

function pow_reset_trigger(){
    jQuery('.pullout-button').unbind('click');
    jQuery('.pullout-button').unbind('mouseover');
}


function get_pow(id){
    for(i in pow_widgets){
        if(pow_widgets[i].id == id){
            return pow_widgets[i];
        }
    }
    return false;
}

function do_pullout(id){
    if( !jQuery.inArray(id, processing) )
        return false;

    if( jQuery('#' + id).is('.pullout-opened') ){
        pow_close(id);
    }else{
        pow_open(id);
    }
}

function pow_open(id){

    var widget = get_pow(id);
    if(!widget)
        return;

    //check if there is opened widget
    if( pow_opened.length > 0 && pow_opened != id ){
        pow_close(pow_opened); //close opened widget
    }

    var pullout_id = '#' + widget.id;
    jQuery(pullout_id).removeClass('pullout-closed');
    jQuery(pullout_id).addClass('pullout-opened');
    jQuery(pullout_id + ' .pullout-button span').html(widget.label_close);

    pow_slide(widget, '+');
    pow_opened = id; // assign new id to opened global
}

function pow_close(id){

    var widget = get_pow(id);

    if(!widget)
        return;

    //check if there is opened widget
    if( pow_opened.length > 0 && pow_opened == id ){
        pow_opened = ''; //close opened widget
    }

    var pullout_id = '#' + widget.id;
    jQuery(pullout_id + ' .pullout-button span').html(widget.label_open);

    processing.push(id);
    pow_slide(widget, '-');

    setTimeout(function(){
        jQuery(pullout_id).removeClass('pullout-opened');
        jQuery(pullout_id).addClass('pullout-closed');
        //remove from processing array
        if(processing.length > 0){
            var i = jQuery.inArray(id, processing);
            if(i != -1)
                processing.splice(i, 1);
        }
    }, parseInt(widget.speed));
}

function pow_slide(widget, direction){

    var properties = new Array;
    properties[widget.side] = direction + '=' + widget.position + 'px';

    var duration    = parseInt(widget.speed); //widget.speed;

    jQuery('#' + widget.id).animate(properties, duration, widget.easing);

}


function pow_start_timer(){
    for(i in pow_widgets){
        var id = pow_widgets[i].id;
        var cookie_name = 'pow_' + id + '_timer';
        var trigger_on = pow_widgets[i].trigger_on;
        if( 'timer' == trigger_on || 'timer_once' == trigger_on ){
            //clear the cookie in case it was set for timer_once and then changed to timer
            if( 'timer' == trigger_on ){
                eraseCookie(cookie_name);
            }
            var startTime = readCookie(cookie_name);
            var delay = parseInt(pow_widgets[i].timer);

            if(!startTime){
                var startTime = new Date();
                createCookie(cookie_name, startTime.getTime(), 2592000); //30 days
                setTimeout(
                    (function(id, cookie_name, trigger_on){
                        return function(){
                            do_pullout(id);
                            if( 'timer_once' == trigger_on ){
                                createCookie(cookie_name, -1, 2592000); //30days
                            }else{
                                eraseCookie(cookie_name);
                            }
                        }
                    })(id, cookie_name, trigger_on), delay);
            }else
            if(startTime != -1){
                var now = new Date();
                var time_passed = (now.getTime() - startTime);

                if( time_passed < delay ){
                    var remaining_t = delay-time_passed;
                    setTimeout(
                        (function(id, cookie_name, trigger_on){
                            return function(){
                                do_pullout(id);
                                if( 'timer_once' == trigger_on ){
                                    createCookie(cookie_name, -1, 2592000); //30days
                                }else{
                                    eraseCookie(cookie_name);
                                }
                            }
                        })(id, cookie_name, trigger_on), remaining_t);
                }else
                if( 3 > (time_passed-delay) ){
                    do_pullout(id);
                    if( 'timer_once' == trigger_on ){
                        createCookie(cookie_name, -1, 2592000); //30days
                    }else{
                        eraseCookie(cookie_name);
                    }
                }
            }
        }
    }
}

function pow_count_pages(){
    var id = pow_widgets[i].id;
    var cookie_name = 'pow_' + id + 'n_pages';
    var num = readCookie(cookie_name);
    if( !num ){
        var num = 1;
    }else{
        num++;
    }
    createCookie(cookie_name, num, 3600); // 1 hour
}

function pow_start_page_count(){
    for(i in pow_widgets){
        var id = pow_widgets[i].id;
        var cookie_name = 'pow_' + id + 'n_pages';
        var trigger_on = pow_widgets[i].trigger_on;
        if( 'n_pages' == trigger_on || 'n_pages_once' == trigger_on ){
            //check if cookie is set
            var num = readCookie(cookie_name);
            if( !num )
                var num = 0;

            num++;
            createCookie(cookie_name, num, 3600); // 1 hour
            if(num == pow_widgets[i].n_pages){
                do_pullout(id);
                if('n_pages' == trigger_on ){
                    eraseCookie(cookie_name);
                }
            }

            if(num > pow_widgets[i].n_pages && 'n_pages' == trigger_on)
                eraseCookie(cookie_name);

        }
    }
}

////////// Cookies Functions

function createCookie(name,value,seconds) {
    if (seconds) {
        var date = new Date();
        date.setTime(date.getTime()+(seconds*1000));
        var expires = "; expires="+date.toGMTString();
    }
    else var expires = "";
    document.cookie = name+"="+value+expires+"; path=/";
}

function readCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for(var i=0;i < ca.length;i++) {
        var c = ca[i];
        while (c.charAt(0)==' ') c = c.substring(1,c.length);
        if (jQuery.inArray(nameEQ, c) == 0) return c.substring(nameEQ.length,c.length);
    }
    return null;
}

function eraseCookie(name) {
    createCookie(name,"",-1);
}