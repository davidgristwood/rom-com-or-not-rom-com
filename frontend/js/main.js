var timeoutID = null;

$(function () {
    $(document).on('input propertychange', "textarea[name='summary']", function (e) {
        clearTimeout(timeoutID);
        timeoutID = setTimeout(() => submitScript($.trim($("#summary").val())), 500);
    });
});

function submitScript(text) {
    // find and list the genres we think it is
    checkGenre(text, (genre) => {
        displayResult(genre);
    });
}

function checkGenre(text, callback) {
    $.ajax("https://romcomnotromcom.azurewebsites.net/predict", {
        data: JSON.stringify(text),
        dataType: 'json',
        contentType: 'application/json',
        type: 'POST',
        success: callback
    });
}

function displayResult(data) {
    let genres = $('#genres');
    genres.empty();

    $.each(data, function(index, item) {
        let probability = Math.ceil(item.probability * 100);
        genres.append($("<div>").addClass('bar').css('width', probability + '%').text(probability + '% ' + item.genre));
    });


}