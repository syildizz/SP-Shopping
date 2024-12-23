// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/**
 * Used to validate if a file is an image. The validation message is set for the "elem" element.
 * @param {HTMLInputElement | HTMLSelectElement | HTMLButtonElement | HTMLOutputElement | HTMLTextAreaElement | HTMLFormElement} elem The element that is going to have its validation message set
 * @param {File} file The file that is going to be validated
 * @param {number} maxImageSize The maximum size permitted for the file
 * @returns The validity of the file
 */
export function validateImageFile(elem, file, maxImageSize) {

    // Validate type
    if (!(file.type.includes("image"))) {
        elem.setCustomValidity("File has to be an image");
        return false;
    }

    // Validate size
    if (!(file.size < 1_500_000)) {
        elem.setCustomValidity(`Cannot upload images larger than ${maxImageSize / 1_000_000.0}MB\n`
            + `Supplied image is ${file.size / 1_000_000.0}MB`);
        return false;
    }

    elem.setCustomValidity("");
    return true;
}

