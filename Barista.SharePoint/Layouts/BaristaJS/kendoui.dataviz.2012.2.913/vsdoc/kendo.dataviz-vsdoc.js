var kendo = {
    ui: {},
    mobile: { ui: {}},
    dataviz: {ui: {}}
};







FX = function() { };

FX.prototype = {



    
    
    
    kendoAnimate: function(effects,duration,reverse,complete,show,hide) {
        /// <summary>
        /// Applies the specified animation effect/s to all selected elements and triggers the callback on every element when it completes its animation.
/// Uses transitions and transformations where available and falls back to jQuery animate where not. kendoAnimate can be used to run one of the provided
/// animation effects or you can define one yourself, using the same format.
        /// </summary>

        /// <param name="effects" type="String|Object" >The effect/s that should be executed on the selected elements. Can be one or several combined effects, specified as a string with format "effect:direction effect:direction". Transformation effects that animate the same property can't be combined, like slide:left and slide:right for instance.</param>

        /// <param name="duration" type="Number" >The effect duration (speed) in milliseconds.</param>

        /// <param name="reverse" type="Boolean" >Whether the effect should play backwards, useful when doing the same animation but with the opposite direction, like opening and closing.</param>

        /// <param name="complete" type="Function" >Completion callback that should be called after animation completion. It gets fired for every animated element and is passed the said element as its only argument.</param>

        /// <param name="show" type="Boolean" >Whether the element should be shown before animating.</param>

        /// <param name="hide" type="Boolean" >Whether the element should be hidden after the animation completes.</param>



        },

    
    
    
    kendoStop: function(clearQueue,gotoEnd) {
        /// <summary>
        /// Stops the animation effect running on the specified elements and optionally jumps to the end and clears the animation effect queue.
/// In browsers that don't support transitions falls back to jQuery stop().elements.kendoStop(clearQueue, gotoEnd);This functionality is useful to avoid chaining many effects, causing them to run longer than expected.
        /// </summary>

        /// <param name="clearQueue" type="Boolean" >Whether to clear the animation effects queue and start anew.</param>

        /// <param name="gotoEnd" type="Boolean" >Whether to jump to the animation end position when stopping or just leave the element at its current position.</param>



        },

    
    
    
    kendoAddClass: function() {
        /// <summary>
        /// Adds a CSS class to the element, while doing a transition to the new state. If the browser doesn't support transitions,
/// the method falls back to jQuery addClass();> **Important:** kendoAddClass doesn't add the animation to the animation effect queue and can't be stopped with kendoStop.elements.kendoAddClass(classes, options);
        /// </summary>



        },

    
    
    
    kendoRemoveClass: function() {
        /// <summary>
        /// Removes a CSS class from the element, while doing a transition to the new state. If the browser doesn't support transitions,
/// the method falls back to jQuery removeClass();> **Important:** kendoRemoveClass doesn't add the animation to the animation effect queue and can't be stopped with kendoStop.elements.kendoRemoveClass(classes, options);
        /// </summary>



        },

    
    
    
    kendoToggleClass: function() {
        /// <summary>
        /// Toggle a CSS class on the element, based on a flag, while doing a transition to the new state. If the browser doesn't support transitions,
/// the method falls back to jQuery toggleClass();> **Important:** kendoToggleClass doesn't add the animation to the animation effect queue and can't be stopped with kendoStop.elements.kendoToggleClass(classes, options, toggle);
        /// </summary>



        },


    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getFX = function() {
    /// <summary>
    /// Returns a reference to the FX widget, instantiated on the selector.
    /// </summary>
    /// <returns type="FX">The FX instance (if present).</returns>
};

$.fn.FX = function(options) {
    /// <summary>
    /// Instantiates a FX widget based the DOM elements that match the selector.
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};








kendo = {};






kendo.Drag = function() { };

kendo.Drag.prototype = {



    
    
    
    cancel: function() {
        /// <summary>
        /// Discard the current drag. Calling the `cancel` method will trigger the `cancel` event.
/// The correct moment to call this method would be in the `start` event handler.
        /// </summary>



        },

    
    
    
    capture: function() {
        /// <summary>
        /// Capture the current drag, so that Drag listeners bound to parent elements will not trigger.
/// This method will not have any effect if the current drag instance is instantiated with the `global` option set to true.
        /// </summary>



        },


    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDrag = function() {
    /// <summary>
    /// Returns a reference to the kendo.Drag widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.Drag">The kendo.Drag instance (if present).</returns>
};

$.fn.kendoDrag = function(options) {
    /// <summary>
    /// Instantiates a kendo.Drag widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// &#10;allowSelection — Boolean (default: false)
    ///&#10;If set to true, the mousedown and selectstart events will not be prevented.
    ///&#10;
    /// &#10;filter — Selector undefined
    ///&#10;If passed, the filter limits the child elements that will trigger the event sequence.
    ///&#10;
    /// &#10;global — Boolean (default: false)
    ///&#10;If set to true, the drag event will be tracked beyond the element boundaries.
    ///&#10;
    /// &#10;stopPropagation — Boolean (default: false)
    ///&#10;If set to true, the mousedown event propagation will stopped, disabling
/// &#10;drag capturing at parent elements.
/// &#10;If set to false, dragging outside of the element boundaries will trigger the `end` event.
    ///&#10;
    /// &#10;surface — Element undefined
    ///&#10;If set, the drag event will be tracked for the surface boundaries. By default, leaving the element boundaries will end the drag.
    ///&#10;
    /// &#10;threshold — Number (default: 0)
    ///&#10;The minimum distance the mouse/touch should move before the event is triggered.
    ///&#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.DragAxis = function() { };

kendo.DragAxis.prototype = {




    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDragAxis = function() {
    /// <summary>
    /// Returns a reference to the kendo.DragAxis widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.DragAxis">The kendo.DragAxis instance (if present).</returns>
};

$.fn.kendoDragAxis = function(options) {
    /// <summary>
    /// Instantiates a kendo.DragAxis widget based the DOM elements that match the selector.
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.data.DataSource = function() { };

kendo.data.DataSource.prototype = {



    
    
    
    add: function() {
        /// <summary>
        /// Adds a new data item to the DataSource.
        /// </summary>


        /// <returns type="kendo.data.Model">The instance which has been added.</returns>


        },

    
    
    
    aggregate: function(val) {
        /// <summary>
        /// Get current aggregate descriptors or applies aggregates to the data.
        /// </summary>

        /// <param name="val" type="Object|Array" >Aggregate(s) to be applied to the data.</param>


        /// <returns type="Array">Current aggregate descriptors</returns>


        },

    
    
    
    aggregates: function() {
        /// <summary>
        /// Get result of aggregates calculation
        /// </summary>


        /// <returns type="Array">Aggregates result</returns>


        },

    
    
    
    at: function(index) {
        /// <summary>
        /// Returns the data item at the specified index.
        /// </summary>

        /// <param name="index" type="Number" >The zero-based index of the data item.</param>


        /// <returns type="kendo.data.ObservableObject | kendo.data.Model">The type depends on the schema.</returns>


        },

    
    
    
    cancelChanges: function() {
        /// <summary>
        /// Cancel the changes made to the DataSource after the last sync. Any changes currently existing in the model
/// will be discarded.
        /// </summary>



        },

    
    
    
    data: function(value) {
        /// <summary>
        /// Gets or sets the data of the `DataSource`.
        /// </summary>

        /// <param name="value" type="Array" >An `Array` of items to set as the current data of the `DataSource`. If omitted the current data will be returned.</param>


        /// <returns type="ObservableArray` the items of the `DataSource"></returns>


        },

    
    
    
    fetch: function(callback) {
        /// <summary>
        /// Fetches data using the current filter/sort/group/paging information.
/// If data is not available and remote operations are enabled data is requested through the transport,
/// otherwise operations are executed over the available data.
        /// </summary>

        /// <param name="callback" type="Function" >Optional callback which will be executed when the data is ready.</param>



        },

    
    
    
    filter: function(filters) {
        /// <summary>
        /// Get current filters or filter the data._Supported filter operators/aliases are_:
        /// </summary>

        /// <param name="filters" type="Object|Array" >Filter(s) to be applied to the data.</param>


        /// <returns type="Array">The current filter descriptors.</returns>


        },

    
    
    
    get: function(id) {
        /// <summary>
        /// Retrieves a model instance by given id.
        /// </summary>

        /// <param name="id" type="Number|String" >The id of the model to be retrieved. The id of the model is defined via `schema.model.id`.</param>


        /// <returns type="kendo.data.Model` the model instance. If not found `undefined">is returned.</returns>


        },

    
    
    
    getByUid: function(uid) {
        /// <summary>
        /// Retrieves a data item by its [uid](/api/framework/observableobject#uid) field.
        /// </summary>

        /// <param name="uid" type="String" >The uid of the item to be retrieved</param>


        /// <returns type="kendo.data.ObservableObject` or `kendo.data.Model` (if `schema.model` is set). If not found `undefined">is returned.</returns>


        },

    
    
    
    group: function(groups) {
        /// <summary>
        /// Get current group descriptors or group the data.
        /// </summary>

        /// <param name="groups" type="Object|Array" >Group(s) to be applied to the data.</param>


        /// <returns type="Array">The current group descriptors.</returns>


        },

    
    
    
    insert: function(index) {
        /// <summary>
        /// Inserts a new data item in the DataSource.
        /// </summary>

        /// <param name="index" type="Number" >The zer-based index at which the data item will be inserted</param>


        /// <returns type="kendo.data.Model">The instance which has been inserted.</returns>


        },

    
    
    
    page: function(page) {
        /// <summary>
        /// Get current page index or request a page with specified index.
        /// </summary>

        /// <param name="page" type="Number" >The index of the page to be retrieved</param>


        /// <returns type="Number">Current page index</returns>


        },

    
    
    
    pageSize: function(size) {
        /// <summary>
        /// Get current pageSize or request a page with specified number of records.
        /// </summary>

        /// <param name="size" type="Number" >The of number of records to be retrieved.</param>


        /// <returns type="Number">Current page size</returns>


        },

    
    
    
    query: function(options) {
        /// <summary>
        /// Executes a query over the data. Available operations are paging, sorting, filtering, grouping.
/// If data is not available or remote operations are enabled, data is requested through the transport.
/// Otherwise operations are executed over the available data.
        /// </summary>

        /// <param name="options" type="Object" >Contains the settings for the operations.</param>



        },

    
    
    
    read: function(data) {
        /// <summary>
        /// Read data into the DataSource using the `transport.read` setting.
        /// </summary>

        /// <param name="data" type="Object" >Optional data to pass to the remote service configured via `transport.read`.</param>



        },

    
    
    
    remove: function(model) {
        /// <summary>
        /// Remove a given `kendo.data.Model` instance from the DataSource.
        /// </summary>

        /// <param name="model" type="Object" >The [kendo.data.Model](/api/framework/model) instance to be removed.</param>



        },

    
    
    
    sort: function(sort) {
        /// <summary>
        /// Get current sort descriptors or sorts the data.
        /// </summary>

        /// <param name="sort" type="Object | Array" >Sort options to be applied to the data</param>


        /// <returns type="Array">the current sort descriptors.</returns>


        },

    
    
    
    sync: function() {
        /// <summary>
        /// Synchronizes changes through the transport. Any pending CRUD operations will be sent to the server.
/// If the DataSource is in **batch** mode, only one call will be made for each type of operation (Create, Update, Destroy).
/// Otherwise, the DataSource will send one request per item change and change type.
        /// </summary>



        },

    
    
    
    total: function() {
        /// <summary>
        /// Get the total number of data items.
        /// </summary>


        /// <returns type="Number">the number of data items.</returns>


        },

    
    
    
    totalPages: function() {
        /// <summary>
        /// Get the number of available pages.
        /// </summary>


        /// <returns type="Number">the available pages.</returns>


        },

    
    
    
    view: function() {
        /// <summary>
        /// Returns a the current state of the data items - with applied paging, sorting, filtering and grouping.To ensure that data is available this method should be use from within `change` event of the DataSource.
        /// </summary>


        /// <returns type="kendo.data.ObservableArary">the data items.</returns>


        },


    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDataDataSource = function() {
    /// <summary>
    /// Returns a reference to the kendo.data.DataSource widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.data.DataSource">The kendo.data.DataSource instance (if present).</returns>
};

$.fn.kendoDataDataSource = function(options) {
    /// <summary>
    /// Instantiates a kendo.data.DataSource widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// &#10;aggregate — Array | Object (default: undefined)
    ///&#10;Sets fields on which initial aggregates should be calculated
    ///&#10;
    /// &#10;autoSync — Boolean undefined
    ///&#10;Enables (*true*) or disables (*false*) the automatic invocation of the sync() method for each change made.
    ///&#10;
    /// &#10;batch — Boolean undefined
    ///&#10;Enables (*true*) or disables (*false*) batch mode.
    ///&#10;
    /// &#10;data — Array undefined
    ///&#10;Specifies the local JavaScript object to use for the data source.
    ///&#10;
    /// &#10;filter — Array | Object (default: undefined)
    ///&#10;Sets initial filter
    ///&#10;
    /// &#10;group — Array | Object (default: undefined)
    ///&#10;Sets initial grouping
    ///&#10;
    /// &#10;page — Number (default: undefined)
    ///&#10;Sets the index of the displayed page of data.
    ///&#10;
    /// &#10;pageSize — Number (default: undefined)
    ///&#10;Sets the number of records which contains a given page of data.
    ///&#10;
    /// &#10;schema — Object undefined
    ///&#10;Set the object responsible for describing the raw data format.
    ///&#10;
    /// &#10;serverAggregates — Boolean (default: false)
    ///&#10;Determines if aggregates are calculated on the server or not. By default aggregates are calculated client-side.
    ///&#10;
    /// &#10;serverFiltering — Boolean (default: false)
    ///&#10;Determines if filtering of the data is handled on the server. By default filtering is performed client-side.> **Important:** When `serverFiltering` is set to `true` the developer is responsible for filtering the data.By default, a filter object is sent to the server with the query string in the following form:*   filter[logic]: and
/// &#10;*   filter[filters][0][field]: orderId
/// &#10;*   filter[filters][0][operator]: desc
/// &#10;*   filter[filters][0][value]: 10248Possible values for **operator** include:
    ///&#10;
    /// &#10;serverGrouping — Boolean (default: false)
    ///&#10;Determines if grouping of the data is handled on the server. By default grouping is performed client-side.> **Important:** When `serverGrouping` is set to `true` the developer is responsible for grouping the data.By default, a group object is sent to the server with the query string in the following form:*   group[0][field]: orderId
/// &#10;*   group[0][dir]: descIt is possible to modify these parameters by using the `parameterMap` function found on the [transport](#transport-object).
    ///&#10;
    /// &#10;serverPaging — Boolean (default: false)
    ///&#10;Determines if paging of the data is on the server. By default paging is performed client-side. If `serverPaging` is enabled the
/// &#10;total number of data items should also be returned in the response. Use the `schema.total` setting to customize that.> **Important:** When `serverPaging` is set to `true` the developer is responsible for paging the data.The following options are sent to the server as part of the query string by default:
    ///&#10;
    /// &#10;serverSorting — Boolean (default: false)
    ///&#10;Determines if sorting of the data should is handled on the server. By default sorting is performed client-side.> **Important:** When `serverSorting` is set to `true` the developer is responsible for sorting the data.By default, a sort object is sent to the server with the query string in the following form:*   sort[0][field]: orderId
/// &#10;*   sort[0][dir]: ascIt is possible to modify these parameters by using the `parameterMap` function found on the [transport](#transport-object).
    ///&#10;
    /// &#10;sort — Array | Object (default: undefined)
    ///&#10;Sets initial sort order
    ///&#10;
    /// &#10;transport — Object undefined
    ///&#10;Specifies the settings for loading and saving data. This can be a remote or local/in-memory data.
    ///&#10;
    /// &#10;type — String undefined
    ///&#10;Loads transport with preconfigured settings. Currently supports only "odata" (Requires kendo.data.odata.js to be included).
    ///&#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.data.HierarchicalDataSource = function() { };

kendo.data.HierarchicalDataSource.prototype = {




    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDataHierarchicalDataSource = function() {
    /// <summary>
    /// Returns a reference to the kendo.data.HierarchicalDataSource widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.data.HierarchicalDataSource">The kendo.data.HierarchicalDataSource instance (if present).</returns>
};

$.fn.kendoDataHierarchicalDataSource = function(options) {
    /// <summary>
    /// Instantiates a kendo.data.HierarchicalDataSource widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.data.Model = function() { };

kendo.data.Model.prototype = {



    
    
    
    bind: function(eventName,handler) {
        /// <summary>
        /// Attaches an event handler for the specified event. Inherited from `ObservableObject`. More info can be found in the [bind](/api/framework/observableobject#bind) section of the
/// ObservableObject API reference.
        /// </summary>

        /// <param name="eventName" type="String" >The name of the event.</param>

        /// <param name="handler" type="Function" >The function which will be invoked when the event is raised.</param>



        },

    
    
    
    Model: function(options) {
        /// <summary>
        /// Defines a new `Model` type using the provided options.
        /// </summary>

        /// <param name="options" type="Object" >Describes the configuration options of the new model type.</param>



        },

    
    
    
    editable: function(field) {
        /// <summary>
        /// Determines if the specified field is editable or not.
        /// </summary>

        /// <param name="field" type="String" >The field to check.</param>



        },

    
    
    
    get: function() {
        /// <summary>
        /// Gets the value of the specified field. Inherited from `ObservableObject`. More info can be found in the [get](/api/framework/observableobject#get) section of the
/// ObservableObject API reference.
        /// </summary>



        },

    
    
    
    isNew: function() {
        /// <summary>
        /// Checks if the `Model` is new or not. The `id` field is used to determine if a model instance is new or existing one.
/// If the value of the field specified is equal to the default value (specifed through the `fields` configuration) the model is considered as new.
        /// </summary>



        },

    
    
    
    set: function() {
        /// <summary>
        /// Sets the value of the specified field. Inherited from `ObservableObject`. More info can be found in the [set](/api/framework/observableobject#set) section of the
/// ObservableObject API reference.
        /// </summary>



        },

    
    
    
    toJSON: function() {
        /// <summary>
        /// Creates a plain JavaScript object which contains all fields of the `Model`. Inherited from `ObservableObject`. More info can be found in the [toJSON](/api/framework/observableobject#tojson) section of the
/// ObservableObject API reference.
        /// </summary>



        },


    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDataModel = function() {
    /// <summary>
    /// Returns a reference to the kendo.data.Model widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.data.Model">The kendo.data.Model instance (if present).</returns>
};

$.fn.kendoDataModel = function(options) {
    /// <summary>
    /// Instantiates a kendo.data.Model widget based the DOM elements that match the selector.
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.data.Node = function() { };

kendo.data.Node.prototype = {



    
    
    
    append: function(model) {
        /// <summary>
        /// Appends a new item to the children datasource, and initializes the datasource, if necessary.
        /// </summary>

        /// <param name="model" type="Object" >The data for the new item</param>



        },

    
    
    
    level: function() {
        /// <summary>
        /// Gets the current nesting level of the Node within the HierarchicalDataSource.var dataSource = new HierarchicalDataSource({
/// data: [
/// { id: 1, text: "Root", items: [
/// { id: 2, text: "Child" }
/// ] }
/// ]
/// });dataSource.read();var root = dataSource.get(1);
/// equals(root.level(), 0);root.load(); // Load child nodesvar child = dataSource.get(2);
/// equals(child.level(), 1);
        /// </summary>



        },

    
    
    
    load: function() {
        /// <summary>
        /// Loads the child nodes in the child datasource, supplying the `id` of the Node to the request.
        /// </summary>



        },

    
    
    
    loaded: function() {
        /// <summary>
        /// Gets or sets the loaded flag of the Node. Setting the loaded flag to `false` allows reloading of child items.
        /// </summary>



        },

    
    
    
    parentNode: function() {
        /// <summary>
        /// Gets the parent node of the Node, if any.
        /// </summary>



        },


    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDataNode = function() {
    /// <summary>
    /// Returns a reference to the kendo.data.Node widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.data.Node">The kendo.data.Node instance (if present).</returns>
};

$.fn.kendoDataNode = function(options) {
    /// <summary>
    /// Instantiates a kendo.data.Node widget based the DOM elements that match the selector.
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.data.ObservableArray = function() { };

kendo.data.ObservableArray.prototype = {



    
    
    
    bind: function(eventName,handler) {
        /// <summary>
        /// Attaches an event handler for the specified event.
        /// </summary>

        /// <param name="eventName" type="String" >The name of the event.</param>

        /// <param name="handler" type="Function" >The function which will be invoked when the event is raised.</param>



        },

    
    
    
    join: function(separator) {
        /// <summary>
        /// Joins all items of an `ObservableArray` into a string. Equivalent of
/// [Array.prototype.join](http://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Array/join).
        /// </summary>

        /// <param name="separator" type="String" >Specifies the string to separate each item of the array. If omitted the array items are separated with a comma (`,`)</param>



        },

    
    
    
    parent: function() {
        /// <summary>
        /// Returns the parent `ObservableObject`. If the current `ObservableArray` is not nested
/// returns `undefined`.
        /// </summary>



        },

    
    
    
    pop: function() {
        /// <summary>
        /// Removes the last item from an array and returns that item. Equivalent of
/// [Array.prototype.pop](http://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Array/pop).
        /// </summary>



        },

    
    
    
    push: function() {
        /// <summary>
        /// Appends the given items to the array and returns the new length of the array. Equivalent of
/// [Array.prototype.push](http://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Array/push).
/// The new items are wrapped as `ObservableObject` if they are complex objects.
        /// </summary>



        },

    
    
    
    slice: function(begin,end) {
        /// <summary>
        /// Returns a one-level deep copy of a portion of an array. Equivalent of
/// [Array.prototype.slice](http://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Array/slice).
/// The result of the `slice` method is **not** an instance of `ObvservableArray`. It is a regular JavaScript Array object.
/// > **Important:** The `slice` method does not modify the original `ObservableArray`.
        /// </summary>

        /// <param name="begin" type="Number" >Zero-based index at which to begin extraction.</param>

        /// <param name="end" type="Number" >Zero-based index at which to end extraction. If `end` is omitted, `slice` extracts to the end of the sequence.</param>



        },

    
    
    
    splice: function(index,howMany) {
        /// <summary>
        /// Changes an `ObservableArray`, by adding new items while removing old items. Equivalent of
/// [Array.prototype.splice](http://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Array/splice)
        /// </summary>

        /// <param name="index" type="Number" >Index at which to start changing the array. If negative, will begin that many elements from the end.</param>

        /// <param name="howMany" type="Number" >An integer indicating the number of items to remove. If howMany is 0, no items are removed. In this case, you should specify at least one new item.</param>


        /// <returns type="An `Array` containing the removed items. The result of the `splice` method is **not** an instance of `ObvservableArray">.</returns>


        },

    
    
    
    shift: function() {
        /// <summary>
        /// Removes the first item from an `ObvservableArray` and returns that item. Equivalent of
/// [Array.prototype.shift](http://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Array/shift).
        /// </summary>



        },

    
    
    
    toJSON: function() {
        /// <summary>
        /// Returns a JavaScript Array which represents the contents of the `ObservableArray`.
        /// </summary>



        },

    
    
    
    unshift: function() {
        /// <summary>
        /// Adds one or more items to the beginning of an `ObservableArray` and returns the new length.
/// Equivalent of [Array.prototype.unshift](http://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Array/unshift).
        /// </summary>



        },


    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDataObservableArray = function() {
    /// <summary>
    /// Returns a reference to the kendo.data.ObservableArray widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.data.ObservableArray">The kendo.data.ObservableArray instance (if present).</returns>
};

$.fn.kendoDataObservableArray = function(options) {
    /// <summary>
    /// Instantiates a kendo.data.ObservableArray widget based the DOM elements that match the selector.
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.data.ObservableObject = function() { };

kendo.data.ObservableObject.prototype = {



    
    
    
    bind: function(eventName,handler) {
        /// <summary>
        /// Attaches an event handler for the specified event.
        /// </summary>

        /// <param name="eventName" type="String" >The name of the event.</param>

        /// <param name="handler" type="Function" >The function which will be invoked when the event is raised.</param>



        },

    
    
    
    get: function(name) {
        /// <summary>
        /// Gets the value of the specified field.
        /// </summary>

        /// <param name="name" type="String" >The name of the field whose value is going to be returned.</param>


        /// <returns type="Object">The value of the specified field.</returns>


        },

    
    
    
    parent: function() {
        /// <summary>
        /// Returns the parent `ObservableObject`. If the current `ObservableObject` is not
/// nested returns `undefined`;
        /// </summary>



        },

    
    
    
    set: function(name,value) {
        /// <summary>
        /// Sets the value of the specified field.
        /// </summary>

        /// <param name="name" type="String" >The name of the field whose value is going to be returned.</param>

        /// <param name="value" type="Number|String|Date|Object" >The new value of the field.</param>



        },

    
    
    
    toJSON: function() {
        /// <summary>
        /// Creates a plain JavaScript object which contains all fields of the `ObservableObject`.
        /// </summary>


        /// <returns type="An `Object` which contains only the fields of the `ObservableObject">.</returns>


        },


    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDataObservableObject = function() {
    /// <summary>
    /// Returns a reference to the kendo.data.ObservableObject widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.data.ObservableObject">The kendo.data.ObservableObject instance (if present).</returns>
};

$.fn.kendoDataObservableObject = function(options) {
    /// <summary>
    /// Instantiates a kendo.data.ObservableObject widget based the DOM elements that match the selector.
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.dataviz.ui.Chart = function() { };

kendo.dataviz.ui.Chart.prototype = {



    
    
    
    destroy: function() {
        /// <summary>
        /// Prepares the Chart for safe removal from the DOM.Detaches event handlers and removes data entries in order to avoid memory leaks.
        /// </summary>



        },

    
    
    
    refresh: function() {
        /// <summary>
        /// Reloads the data and repaints the chart.
        /// </summary>



        },

    
    
    
    svg: function() {
        /// <summary>
        /// Returns the SVG representation of the current chart.
/// The returned string is a self-contained SVG document
/// that can be used as is or converted to other formats
/// using tools like [Inkscape](http://inkscape.org/) and
/// [ImageMagick](http://www.imagemagick.org/).
/// Both programs provide command-line interface
/// suitable for backend processing.
        /// </summary>



        },


    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDatavizChart = function() {
    /// <summary>
    /// Returns a reference to the kendo.dataviz.ui.Chart widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.dataviz.ui.Chart">The kendo.dataviz.ui.Chart instance (if present).</returns>
};

$.fn.kendoDatavizChart = function(options) {
    /// <summary>
    /// Instantiates a kendo.dataviz.ui.Chart widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// &#10;axisDefaults — Object undefined
    ///&#10;Default options for all chart axes.
    ///&#10;
    /// &#10;categoryAxis — Object undefined
    ///&#10;The category axis configuration options.
    ///&#10;
    /// &#10;chartArea — Object undefined
    ///&#10;The chart area configuration options.
/// &#10;This is the entire visible area of the chart.
    ///&#10;
    /// &#10;dataSource — Object undefined
    ///&#10;DataSource configuration or instance.
    ///&#10;
    /// &#10;legend — Object undefined
    ///&#10;The chart legend configuration options.
    ///&#10;
    /// &#10;plotArea — Object undefined
    ///&#10;The plot area configuration options. This is the area containing the plotted series.
    ///&#10;
    /// &#10;series — Array undefined
    ///&#10;Array of series definitions.The series type is determined by the value of the type field.
/// &#10;If a type value is missing, the type is assumed to be the one specified in seriesDefaults.Each series type has a different set of options.
    ///&#10;
    /// &#10;seriesColors — Array undefined
    ///&#10;The default colors for the chart's series. When all colors are used, new colors are pulled from the start again.
    ///&#10;
    /// &#10;seriesDefaults — Object undefined
    ///&#10;Default values for each series.
    ///&#10;
    /// &#10;theme — String undefined
    ///&#10;Sets Chart theme. Available themes: default, blueOpal, black.
    ///&#10;
    /// &#10;title — Object undefined
    ///&#10;The chart title configuration options.
    ///&#10;
    /// &#10;tooltip — Object undefined
    ///&#10;The data point tooltip configuration options.
    ///&#10;
    /// &#10;transitions — Boolean (default: true)
    ///&#10;A value indicating if transition animations should be played.
    ///&#10;
    /// &#10;valueAxis — Object undefined
    ///&#10;The value axis configuration options.
    ///&#10;
    /// &#10;xAxis — Object undefined
    ///&#10;Scatter charts X-axis configuration options.
/// &#10;Includes **all valueAxis options** in addition to:
    ///&#10;
    /// &#10;yAxis — Object undefined
    ///&#10;The scatter charts Y-axis configuration options.
/// &#10;See **xAxis** for list of available options.
    ///&#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.dataviz.ui.LinearGauge = function() { };

kendo.dataviz.ui.LinearGauge.prototype = {



    
    
    
    destroy: function() {
        /// <summary>
        /// Prepares the Gauge for safe removal from the DOM.Detaches event handlers and removes data entries in order to avoid memory leaks.
        /// </summary>



        },

    
    
    
    redraw: function() {
        /// <summary>
        /// Redraws the gauge.
        /// </summary>



        },

    
    
    
    svg: function() {
        /// <summary>
        /// Returns the SVG representation of the current gauge.
/// The returned string is a self-contained SVG document
/// that can be used as is or converted to other formats
/// using tools like [Inkscape](http://inkscape.org/) and
/// [ImageMagick](http://www.imagemagick.org/).
/// Both programs provide command-line interface
/// suitable for backend processing.
        /// </summary>



        },

    
    
    
    value: function() {
        /// <summary>
        /// Change the value of the gauge.
        /// </summary>



        },


    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDatavizLinearGauge = function() {
    /// <summary>
    /// Returns a reference to the kendo.dataviz.ui.LinearGauge widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.dataviz.ui.LinearGauge">The kendo.dataviz.ui.LinearGauge instance (if present).</returns>
};

$.fn.kendoDatavizLinearGauge = function(options) {
    /// <summary>
    /// Instantiates a kendo.dataviz.ui.LinearGauge widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// &#10;gaugeArea — Object undefined
    ///&#10;The gauge area configuration options.
/// &#10;This is the entire visible area of the gauge.
    ///&#10;
    /// &#10;pointer — Object undefined
    ///&#10;The pointer configuration options.
    ///&#10;
    /// &#10;scale — Object undefined
    ///&#10;Configures the scale.
    ///&#10;
    /// &#10;transitions — Boolean (default: true)
    ///&#10;A value indicating if transition animations should be played.
    ///&#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.dataviz.ui.RadialGauge = function() { };

kendo.dataviz.ui.RadialGauge.prototype = {



    
    
    
    destroy: function() {
        /// <summary>
        /// Prepares the Gauge for safe removal from the DOM.Detaches event handlers and removes data entries in order to avoid memory leaks.
        /// </summary>



        },

    
    
    
    redraw: function() {
        /// <summary>
        /// Redraws the gauge.
        /// </summary>



        },

    
    
    
    svg: function() {
        /// <summary>
        /// Returns the SVG representation of the current gauge.
/// The returned string is a self-contained SVG document
/// that can be used as is or converted to other formats
/// using tools like [Inkscape](http://inkscape.org/) and
/// [ImageMagick](http://www.imagemagick.org/).
/// Both programs provide command-line interface
/// suitable for backend processing.
        /// </summary>



        },

    
    
    
    value: function() {
        /// <summary>
        /// Change the value of the gauge.
        /// </summary>



        },


    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDatavizRadialGauge = function() {
    /// <summary>
    /// Returns a reference to the kendo.dataviz.ui.RadialGauge widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.dataviz.ui.RadialGauge">The kendo.dataviz.ui.RadialGauge instance (if present).</returns>
};

$.fn.kendoDatavizRadialGauge = function(options) {
    /// <summary>
    /// Instantiates a kendo.dataviz.ui.RadialGauge widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// &#10;gaugeArea — Object undefined
    ///&#10;The gauge area configuration options.
/// &#10;This is the entire visible area of the gauge.
    ///&#10;
    /// &#10;pointer — Object undefined
    ///&#10;The pointer configuration options.
    ///&#10;
    /// &#10;rangeSize — Number undefined
    ///&#10;The width of the range indicators.
    ///&#10;
    /// &#10;scale — Object undefined
    ///&#10;Configures the scale.
    ///&#10;
    /// &#10;transitions — Boolean (default: true)
    ///&#10;A value indicating if transition animations should be played.
    ///&#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.ui.Draggable = function() { };

kendo.ui.Draggable.prototype = {




    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDraggable = function() {
    /// <summary>
    /// Returns a reference to the kendo.ui.Draggable widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.ui.Draggable">The kendo.ui.Draggable instance (if present).</returns>
};

$.fn.kendoDraggable = function(options) {
    /// <summary>
    /// Instantiates a kendo.ui.Draggable widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// &#10;axis — String (default: null)
    ///&#10;Constrains the hint movement to either the horizontal (x) or vertical (y) axis. Can be set to either "x" or "y".
    ///&#10;
    /// &#10;container — jQuery undefined
    ///&#10;If set, the hint movement is constrained to the container boundaries.
    ///&#10;
    /// &#10;cursorOffset — Object (default: null)
    ///&#10;If set, specifies the offset of the hint relative to the mouse cursor/finger.
/// &#10;By default, the hint is initially positioned on top of the draggable source offset. The option accepts an object with two keys: `top` and `left`.
    ///&#10;
    /// &#10;distance — Number (default: 5)
    ///&#10;The required distance that the mouse should travel in order to initiate a drag.
    ///&#10;
    /// &#10;filter — Selector undefined
    ///&#10;Selects child elements that are draggable if a widget is attached to a container.
    ///&#10;
    /// &#10;group — String (default: "default")
    ///&#10;Used to group sets of draggable and drop targets. A draggable with the same group value as a drop target will be accepted by the drop target.
    ///&#10;
    /// &#10;hint — Function | jQuery undefined
    ///&#10;Provides a way for customization of the drag indicator. If a function is supplied, it receives one argument - the draggable element's jQuery object.
    ///&#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.ui.DropTarget = function() { };

kendo.ui.DropTarget.prototype = {




    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoDropTarget = function() {
    /// <summary>
    /// Returns a reference to the kendo.ui.DropTarget widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.ui.DropTarget">The kendo.ui.DropTarget instance (if present).</returns>
};

$.fn.kendoDropTarget = function(options) {
    /// <summary>
    /// Instantiates a kendo.ui.DropTarget widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// &#10;group — String (default: "default")
    ///&#10;Used to group sets of draggable and drop targets. A draggable with the same group value as a drop target will be accepted by the drop target.
    ///&#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};







kendo.ui.Validator = function() { };

kendo.ui.Validator.prototype = {



    
    
    
    errors: function() {
        /// <summary>
        /// Get the error messages if any.
        /// </summary>


        /// <returns type="Array">Messages for the failed validation rules.</returns>


        },

    
    
    
    validate: function() {
        /// <summary>
        /// Validates the input element(s) against the declared validation rules.
        /// </summary>


        /// <returns type="Boolean` `true">if all validation rules passed successfully.</returns>


        },

    
    
    
    validateInput: function(input) {
        /// <summary>
        /// Validates the input element against the declared validation rules.
        /// </summary>

        /// <param name="input" type="Element" domElement="true">Input element to be validated.</param>


        /// <returns type="Boolean` `true">if all validation rules passed successfully.</returns>


        },


    
    bind: function(event, callback) {
        /// <summary>
        /// Binds to a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be executed when the event is triggered.</param>
    },
    

    
    unbind: function(event, callback) {
        /// <summary>
        /// Unbinds a callback from a widget event.
        /// </summary>
        /// <param name="event" type="String">The event name</param>
        /// <param name="callback" type="Function">The callback to be removed.</param>
    }
    

};

$.fn.getKendoValidator = function() {
    /// <summary>
    /// Returns a reference to the kendo.ui.Validator widget, instantiated on the selector.
    /// </summary>
    /// <returns type="kendo.ui.Validator">The kendo.ui.Validator instance (if present).</returns>
};

$.fn.kendoValidator = function(options) {
    /// <summary>
    /// Instantiates a kendo.ui.Validator widget based the DOM elements that match the selector.
    
    /// &#10;Accepts an object with the following configuration options:
    /// &#10;
    /// &#10;messages — Object undefined
    ///&#10;Set of messages (either strings or functions) which will be shown when given validation rule fails.
/// &#10;By setting already existing key the appropriate built-in message will be overridden.
    ///&#10;
    /// &#10;rules — Object undefined
    ///&#10;Set of custom validation rules. Those rules will extend the [built-in ones](/getting-started/framework/validator/overview#default-validation-rules).
    ///&#10;
    /// &#10;validateOnBlur — Boolean undefined
    ///&#10;Determines if validation will be triggered when element loses focus. Default value is true.
    ///&#10;
    /// </summary>
    /// <param name="options" type="Object">
    /// The widget configuration options
    /// </param>
};





// vim:ft=javascript
