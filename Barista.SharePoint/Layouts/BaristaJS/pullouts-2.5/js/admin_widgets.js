var widget_id = 'widget';
var current_pow_id = '';
var current_pow_wrap_id = '';
var current_dialog = null;
jQuery(document).ready(function(){                    

        var pow_id      = 'pow_options';                
        var current_pow_id = '#' + pow_id;

        jQuery(this).pow_init_icon_selector();        
        pow_init_color_picker();
        jQuery('.pow-accordion').accordion({ autoHeight: false, clearStyle: true, collapsible: true });        
        jQuery('#pow_show_on').change(function(){ pow_toggle_fields(); } );
        jQuery('#pow_show_on').trigger('change'); 

});


function pow_hide_all(){
    var el = ['pow_element', 'pow_timer', 'pow_n_pages'];
    for(i in el){
        jQuery('.'+el[i]).hide();
    }
}

function pow_show(id){
    pow_hide_all();
    jQuery('.'+id).show();
}

function pow_toggle_fields(){          
    var selected = jQuery('#pow_show_on').val();
    
    switch(selected){
        case "appear":        
        case "appear_once":
            pow_show('pow_element');
        break;
        case "timer":        
        case "timer_once":
            pow_show('pow_timer');
        break;
        case "n_pages":        
        case "n_pages_once":
            pow_show('pow_n_pages');
        break;
        case "click":        
        case "mouseover":        
            pow_hide_all();
        break;    
    }
    return false;
}


jQuery.fn.pow_init_icon_selector = function() {
    jQuery(current_pow_id + ' .pow_icon').live('click', function() {             
        var icon_field_id = jQuery(this).attr('rel');
        var selected_icon = jQuery(this).attr('id');
        jQuery('#' + icon_field_id).val(
            selected_icon
        );
        jQuery('#icons .pow_icon').removeClass('pow_icon_selected');                    
        jQuery(this).addClass('pow_icon_selected');
        pow_icon_preview(selected_icon);
    });               
}

function pow_icon_preview(icon) {           
    var coord = icon.split('_');
    var preview = jQuery('.pow_icon_preview');
    var grid = 36;
    var x = grid*(-1) * parseInt(coord[0]) + 7;
    var y = grid*(-1) * parseInt(coord[1]) + 7;            
    
    jQuery('.pow_icon_preview').css({
            'background-position' : x + 'px ' + ' ' +y+'px'
        });
}

function pow_init_color_picker() {    
    jQuery('.pow_color').each(function(){
        var colorpicker_field_id = jQuery(this).attr('id');            
        var colorpicker = jQuery('#colorpicker-' + colorpicker_field_id);
        colorpicker.hide();
        colorpicker.farbtastic('#' + colorpicker_field_id);

        jQuery('#' + colorpicker_field_id).click(function() {        
            colorpicker.fadeIn();
        });        

        jQuery(document).mousedown(function() {
            colorpicker.each(function() {
                var display = jQuery(this).css('display');
                if ( display == 'block' )
                    jQuery(this).fadeOut();
            });
        });

    });        
}   